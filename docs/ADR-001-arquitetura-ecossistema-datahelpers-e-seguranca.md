# ADR 001: Arquitetura Canônica do Ecossistema TL.DataHelpers, Segurança e FinOps

---

## Contexto

A suíte **TL.DataHelpers** (`DataHelpers.sln`) provê componentes de infraestrutura de alta performance para acesso a dados, caching distribuído, auditoria e manipulação de fluxos de dados em .NET 8 e .NET 9.

Com a evolução para a versão 0.3.0, identificou-se a necessidade de sanear débitos arquiteturais e elevar os padrões de segurança e sustentabilidade financeira (FinOps):
1. **Inchaço de Dependências Obsoletas:** A presença de pacotes legados e não-mantidos (`Apache.Ignite`, `EntityFrameworkCore.NCache`, `EnyimMemcachedCore`) agregava mais de 80 MB de dependências transitivas, bibliotecas Java acopladas e arquivos proprietários órfãos (`.ncconf`).
2. **Custo de Infraestrutura para Aplicações Pequenas:** Ambientes de desenvolvimento, testes e microsserviços de baixo volume demandavam a instanciação dispendiosa de clusters Redis para manter a mesma interface de caching.
3. **Riscos de Segurança em Repouso:** A persistência de dados no Redis em formato serializado claro expõe dados sensíveis (PII, tokens de sessão) a dumps de memória, vazamentos de arquivos de persistência (RDB/AOF) ou acesso indevido ao cluster.
4. **Cache Stampede e Avalanches de Expiração:** Em sistemas de alto tráfego, expirações síncronas de chaves quentes sobrecarregam bancos de dados relacionais (*thundering herd*), exigindo coordenação distribuída e dispersão temporal.

---

## Decisões Arquiteturais Canônicas

### 1. Expurgo Definitivo de Dependências Obsoletas
- **Remoção Completa:** Foram expurgadas todas as dependências de `Apache.Ignite`, `EntityFrameworkCore.NCache` e `EnyimMemcachedCore` de `Caching.Helpers.csproj` e `Directory.Packages.props`.
- **Eliminação de Arquivos Proprietários:** Excluídos arquivos órfãos de configuração (`client.ncconf`, `config.ncconf`, `tls.ncconf`).
- **Foco Tecnológico:** A suíte padroniza a persistência L2 distribuída exclusivamente no **Redis** (`Microsoft.Extensions.Caching.StackExchangeRedis`) e a camada L1 em **Memória Local** (`IMemoryCache`).

### 2. Modo Custo Zero ($0) em Memória Local
- **Chaveamento Declarativo:** Através da extensão `AddTlCaching(IConfiguration)`, a biblioteca inspeciona a flag `Cache:UseRedis`.
- **Comportamento In-Memory:** Quando `UseRedis` é `false` ou a string de conexão do Redis não estiver configurada, o sistema ativa transparentemente o `MemoryCacheService` baseado em `IMemoryCache`.
- **API Idêntica:** Aplicações consumidoras utilizam a mesma interface `ICacheService` sem necessitar de contêineres ou instâncias pagas de Redis em ambientes de desenvolvimento e homologação, permitindo economia imediata de custos de nuvem.

### 3. Caching Seguro com Criptografia e Proteção de Dados em Repouso
- **Criptografia em Nível de Aplicação:** Implementado `AesEncryptionProvider` utilizando a classe nativa `System.Security.Cryptography.AesGcm`.
- **Parâmetros Criptográficos:**
  - Chave simétrica de 256 bits (32 bytes).
  - IV/Nonce pseudo-aleatório de 12 bytes gerado via `RandomNumberGenerator`.
  - Tag de autenticação de 16 bytes (128 bits).
- **Proteção contra Adulteração:** Qualquer alteração em bits do texto cifrado, do IV ou da tag resulta imediatamente em `CryptographicException`, impedindo ataques de forja ou adulteração de estado.
- **Limpeza Segura de Memória:** Todos os buffers intermediários contendo dados claros ou chaves são sanitizados com `CryptographicOperations.ZeroMemory` nos blocos `finally`.

### 4. FinOps e Compressão GZip com Reciclagem de Memória
- **Limiar de Compressão (1 KB):** Implementado `GZipCompressionProvider` com ativação automática para payloads serializados $\ge 1.024$ bytes, e bypass inteligente para payloads menores, eliminando desperdício de CPU.
- **Zero-Leak com ArrayPool:** Todos os buffers de I/O são alocados via `ArrayPool<byte>.Shared.Rent()` e devolvidos seguramente no `finally`, erradicando alocações contínuas no Large Object Heap (LOH).
- **Pipeline Ordenado:** Na gravação, a compressão GZip ocorre **antes** da cifragem de dados, maximizando a taxa de redução (mais de 60% de economia de banda de rede e armazenamento no Redis). Na leitura, a decifragem autenticada precede a descompressão.

### 5. Resiliência Distribuída e Prevenção de Cache Stampede
- **TTL Jitter Anti-Avalanche:** O componente `TtlJitterCalculator` aplica dispersão pseudo-aleatória de $\pm 10\%$ a $\pm 15\%$ sobre o tempo de vida base das chaves, distribuindo temporalmente as expirações.
- **Double-Checked Locking Restrito a Miss:** A aquisição de bloqueio distribuído com RedLock (`LockedCacheService` / `CacheLockHelper`) ocorre **exclusivamente sob cache miss** para proteger a invocação da factory do banco de dados.
- **Operações de Escrita Lock-Free:** Métodos `SetAsync`, `RemoveAsync` e `InvalidateRegionAsync` operam inteiramente livres de lock distribuído, garantindo vazão máxima por segundo.
- **Degradação Graciosa (Silent Fallback):** Caso o cluster Redis fique temporariamente inacessível ou ocorra falha de rede, a falha é registrada via `ILogger.LogWarning` e a factory é executada diretamente na fonte de dados, impedindo erros 500 no cliente final.

---

## Decisões Arquiteturais Canônicas — Release v0.4.0 (Persistência, NoSQL & Segurança)

### 6. Desacoplamento de Caching e Especialização em Keyset Seek (`TL.KeysetPagination`)
- **Remoção de Dependências Obsoletas:** Expurgo de `ICacheService` e `Microsoft.Extensions.Caching.*` do pacote de paginação, saneando acoplamentos indevidos de camadas e mantendo foco estrito em paginação.
- **Validação Defensiva de Não-Nulidade:** Em chaves de busca para seek keyset, colunas anuláveis (`Nullable`) disparam `InvalidOperationException`, prevenindo ordenação inconsistente e perda de ponteiro de paginação em grandes volumes de dados.
- **Proteção contra Exposição de Dados via `[FilterIgnore]`:** Atributo de anotação declarativa em propriedades sensíveis ou internas. O parser dinâmico (`DynamicFilterParser`) rejeita requisições com `SecurityException` caso o consumidor tente filtrar ou projetar campos anotados com `[FilterIgnore]`.

### 7. Isolamento NoSQL e Validação Dialetal em Consultas (`TL.QueryBuilder`)
- **Independência de Drivers Externos:** Remoção das referências a `MongoDB.Driver` e `MongoDB.Bson` do `TL.QueryBuilder`. A geração de filtros NoSQL foi reimplementada utilizando recursos nativos da BCL (`System.Text.Json`).
- **Escape Dialetal Rígido:** Criação do `SqlIdentifierValidator` que valida identificadores contra injeções (`^[a-zA-Z_][a-zA-Z0-9_]*$`) e aplica escape automático por dialeto:
  - SQL Server: `[identificador]`
  - PostgreSQL: `"identificador"`
  - MySQL: `` `identificador` ``
- **Proteção contra Sobrecarga em Consultas LIKE:** Interceptação no método `WhereLike` com escape de caracteres coringa (`%`, `_`, `[`) para prevenir consultas com parâmetros arbitrários que forçam varreduras completas de tabelas (*full table scan*) em bancos relacionais.

### 8. Adoção Corporativa de `ILogger` e Sanitização em Exportações (`TL.DataImportExport`)
- **Erradicação de `Console.WriteLine`:** Expurgo de `SimpleLogger` e adoção do `Microsoft.Extensions.Logging.ILogger` padrão da BCL com fallback seguro para `NullLogger.Instance`.
- **Sanitização de Fórmulas em Exportação CSV:** Sanitização preventiva em células de exportação CSV prefixando strings iniciadas por `=`, `+`, `-`, `@`, `\t`, `\r` com apóstrofo seguro (`'`), prevenindo a execução indevida de comandos e fórmulas dinâmicas ao abrir os arquivos em planilhas eletrônicas.
- **Guardrail de Memória no ClosedXML:** Adição de `MaxRowsLimit` (padrão 15.000 linhas) no `ExcelDataImporter`, bloqueando o consumo de planilhas abusivas e orientando o uso do `SpanDelimitedParser` via streaming para grandes volumes.

### 9. Transações ACID Multi-Documento e Idempotência BSON (`TL.MongoDriver`)
- **Transações Explícitas:** Suporte aos métodos `BeginTransactionAsync`, `CommitTransactionAsync` e `RollbackTransactionAsync` gerenciadas pela sessão do Mongo (`IClientSessionHandle`).
- **Rollback Automático Defensivo:** Mecanismo transacional em `SaveChanges()` que aborta automaticamente a transação ativa sob qualquer falha na lista de comandos pendentes antes de relançar a exceção.
- **Registro Idempotente de Convenções e Serializadores:** Introdução do `BsonRegistrationHelper` thread-safe que isola e previne conflitos de concorrência (`BsonSerializationException`) durante a inicialização do container de injeção de dependências.

---

## Consequências

### Impactos Positivos
- **Desacoplamento e Redução de Dependências:** O `QueryBuilder` passa a depender apenas de recursos nativos da BCL (`System.Text.Json`), e a paginação foca estritamente em seu papel sem arrastar abstrações de cache.
- **Previsibilidade e Escalabilidade de Consultas:** A paginação por chaves (Keyset Seek) elimina a sobrecarga de `OFFSET` em tabelas volumosas, mantendo o tempo de consulta estável e indexado.
- **Transacionalidade e Integridade no NoSQL:** O suporte a transações multi-documento com auto-rollback no `MongoContext` garante que falhas parciais não deixem documentos inconsistentes na base.
- **Segurança Nativa por Padrão:** Sanitização transparente contra injeção de fórmulas em CSV, escape automático de identificadores e coringas em SQL dinâmico, e bloqueio de consultas em atributos sensíveis via `[FilterIgnore]`.
- **Padronização de Observabilidade:** Eliminação de saídas diretas em console em favor de `ILogger`, permitindo integração com qualquer provedor de log corporativo.

### Trade-offs e Limitações
- **Navegação por Cursor:** Keyset pagination não permite saltar diretamente para páginas arbitrárias distantes (ex.: ir direto para a página 50), sendo indicada para navegação sequencial contínua e feeds. Para paginação tradicional com totalização, a biblioteca mantém o utilitário baseado em offset.
- **Requisitos de Banco:** Transações multi-documento no MongoDB exigem cluster operando em modo Replica Set.
- **Capacidade de Memória em Planilhas:** A leitura de arquivos Excel pelo ClosedXML mantém o modelo DOM em memória; para arquivos massivos que excedem o guardrail de linhas, deve-se priorizar o uso de CSV via streaming.

---

## Conformidade e Diretrizes Técnicas
- **Proteção de Dados e Defesa em Profundidade:** Cifragem de dados em repouso no cache, bloqueio de exposição de campos sensíveis em APIs e sanitização de dados exportados.
- **Multi-Targeting:** Compilação simultânea e limpa para `.NET 8` e `.NET 9` com política estrita de zero warnings.

