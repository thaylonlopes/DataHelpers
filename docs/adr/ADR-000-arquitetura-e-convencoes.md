# ADR 000: Arquitetura e Convenções da Suíte TL.DataHelpers

---

## Contexto

A suíte **TL.DataHelpers** (solução `DataHelpers.sln`) é um ecossistema modular de componentes utilitários e de infraestrutura em C# / .NET, projetado para simplificar e acelerar o desenvolvimento de aplicações que interagem com bancos de dados relacionais e NoSQL, caching distribuído, paginação de alta performance, mapeamento de objetos, auditoria e importação/exportação de dados.

Composta por 8 projetos de bibliotecas de classes independentes, a suíte visa erradicar duplicação de código e estabelecer padrões consistentes de persistência e performance em microsserviços corporativos. Para assegurar interoperabilidade, confiabilidade, previsibilidade e excelência técnica, este documento formaliza as decisões de arquitetura e governança transversal aplicadas a todo o repositório.

---

## Decisões Arquiteturais e Convenções

### 1. Modularidade Granular e Zero Acoplamento Cruzado
- A solução é composta por 8 módulos granulares independentes:
  - `TL.DataMapping`
  - `TL.Caching.Helpers`
  - `TL.AuditLogger`
  - `TL.Dapper.Helpers`
  - `TL.PagingFiltering.Helpers`
  - `TL.DataImportExport.Helpers`
  - `TL.QueryBuilder.Helpers`
  - `TL.MongoDriver.Helpers`
- Cada pacote possui responsabilidade única e coesa. Nenhum pacote de produção pode referenciar outro pacote de produção irmão (`ProjectReference` entre pacotes é proibido). As aplicações consumidoras instalam estritamente os módulos necessários.

### 2. Multi-Targeting Obrigatório (.NET 8 e .NET 9)
- Todos os pacotes compilam simultaneamente para `.NET 8.0` (LTS) e `.NET 9.0` (STS):
  ```xml
  <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  ```
- **Zero Warnings:** Nenhum aviso de compilação ou de nulidade é tolerado no repositório.
- **Sem NoWarn:** Supressão cega via `<NoWarn>` é estritamente proibida em código de produção.

### 3. Injeção de Dependência Fluente (`Add*`)
- Cada pacote fornece métodos de extensão no namespace `Microsoft.Extensions.DependencyInjection` seguindo a convenção `Add[NomeDoModulo]()`, com suporte a conexão direta e `IConfiguration`.

### 4. Documentação XML Completa da API Pública
- Todos os membros públicos (classes, interfaces, métodos, parâmetros, retornos e exceções) possuem documentação XML completa (`<summary>`, `<param>`, `<returns>`, `<exception>`), garantindo suporte integral a IntelliSense nos ambientes de desenvolvimento.

### 5. Result Pattern e Fail-Fast
- Operações com possibilidade de falha funcional utilizam o padrão monádico `Result<T>` (`IsSuccess`, `Value`, `Error`), evitando exceções caras como controle de fluxo normal.
- Todos os métodos públicos validam imediatamente argumentos não-nulos com Guard Clauses (`ArgumentNullException.ThrowIfNull(param)`).

### 6. Async/Await First e Cancelamento Cooperativo
- Todas as operações de I/O (banco de dados, cache, arquivos e rede) são assíncronas de ponta a ponta, propagando instâncias de `CancellationToken`.

---

## Consequências e Trade-offs

- **Consistência de API:** Experiência homogênea de desenvolvimento e consumo em todos os 8 pacotes da suíte.
- **Portabilidade & Confiabilidade:** Interoperabilidade transparente com ecossistemas .NET 8 e .NET 9 sem riscos de avisos em pipelines de CI/CD.
- **Isolamento Tecnológico:** Liberdade total para atualizar dependências de um pacote sem impacto colateral nos demais.

