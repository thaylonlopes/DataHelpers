# ADR 001: Arquitetura Canônica do Ecossistema TL.DataHelpers, Segurança e FinOps

---

## Status
Aprovado (Versão 0.3.0)

## Contexto

A suíte **TL.DataHelpers** (`DataHelpers.sln`) provê componentes de infraestrutura de alta performance para acesso a dados, caching distribuído, auditoria e manipulação de fluxos de dados em .NET 8 e .NET 9.

Com a evolução para a versão 0.3.0, identificou-se a necessidade de sanear débitos arquiteturais e elevar os padrões de segurança e sustentabilidade financeira (FinOps):
1. **Inchaço de Dependências Obsoletas:** A presença de pacotes legados e não-mantidos (`Apache.Ignite`, `EntityFrameworkCore.NCache`, `EnyimMemcachedCore`) agregava mais de 80 MB de dependências transitivas, bibliotecas Java acopladas e arquivos proprietários órfãos (`.ncconf`).
2. **Custo de Infraestrutura para Aplicações Pequenas:** Ambientes de desenvolvimento, testes e microsserviços de baixo volume demandavam a instanciação dispendiosa de clusters Redis para manter a mesma interface de caching.
3. **Riscos de Segurança em Repouso (AppSec / LGPD / PCI-DSS):** A persistência de dados no Redis em formato serializado claro expõe dados sensíveis (PII, tokens de sessão) a dumps de memória, vazamentos de arquivos de persistência (RDB/AOF) ou invasões de cluster.
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

## Consequências e Benefícios

| Aspecto | Antes (v0.2.0) | Depois (v0.3.0) |
| :--- | :--- | :--- |
| **Pegada Binária** | Dependências proprietárias NCache/Ignite (> 80 MB) | Pacote limpo, leve e 100% aderente a padrões abertos do .NET |
| **Custo de Nuvem** | Redis obrigatório para rodar a suíte | Modo Custo Zero ($0) via `IMemoryCache` nativo |
| **Segurança em Repouso** | JSON claro armazenado no Redis | Criptografia e proteção de dados em repouso |
| **Eficiência de Rede** | Payloads trafegados sem otimização de pool | Redução > 60% com GZip e reciclagem via `ArrayPool` |
| **Disponibilidade** | Risco de falha se Redis oscilar | Silent Fallback resiliente sem interrupção de serviço |

---

## Conformidade e Governança
- **LGPD / GDPR / PCI-DSS:** Conformidade comprovada com cifragem de PIIs e sanitização de memória.
- **SemVer:** Release v0.3.0 com retrocompatibilidade preservada na API pública `ICacheService`.
- **Multi-Targeting:** Compilação simultânea e limpa para `.NET 8` e `.NET 9` com política de Zero Warnings.
