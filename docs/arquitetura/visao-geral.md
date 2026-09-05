# 🏛️ Visão Geral da Arquitetura: TL.DataHelpers (DataHelpers)

## 📌 1. Visão Geral e Propósito do Sistema

O **TL.DataHelpers** (solução `DataHelpers.sln`) é uma suíte modular de bibliotecas utilitárias e componentes de infraestrutura construídos em **.NET 8.0 / .NET 9.0 (C# 12)**, projetada para acelerar o desenvolvimento de aplicações corporativas, microsserviços e Web APIs de alta performance.

O propósito central do ecossistema é eliminar a duplicação de código em operações comuns de acesso a dados (SQL e NoSQL), construção dinâmica de consultas, estratégias avançadas de caching multinível, paginação, filtragem, mapeamento de objetos, auditoria de dados e fluxos de importação/exportação de arquivos.

```mermaid
graph TD
    Consumer["Aplicações Consumidoras<br/>(Web APIs, Microsserviços, Workers)"]
    
    subgraph "DataHelpers Ecosystem (.NET 8.0 / .NET 9.0)"
        Mongo["MongoDriver.Helpers<br/>(CQRS, Transações, Interceptors)"]
        Dapper["Dapper.Helpers<br/>(SQL Repositories, Unit of Work, BulkInsert)"]
        Cache["Caching.Helpers<br/>(L1/L2 Hierárquico, Redis, Locks, Throttling)"]
        QB["QueryBuilder.Helpers<br/>(Poliglota SQL & NoSQL)"]
        Page["PagingFiltering.Helpers<br/>(Keyset Seek, Specifications, Dynamic Filter)"]
        IO["DataImportExport.Helpers<br/>(Streaming CSV, Excel, JSON, XML)"]
        Map["DataMapping.Helpers<br/>(Expression Trees & Result Pattern)"]
        Audit["AuditLogger<br/>(Audit Trail & Multi-Storage)"]
    end
    
    subgraph "Bancos de Dados & Provedores Externos"
        MongoDB[("MongoDB")]
        SQLDB[("SQL Server / Postgres / MySQL / Oracle")]
        Redis[("Redis Cache")]
        Files["Arquivos (CSV, XLSX, JSON, XML)"]
    end

    Consumer --> Mongo
    Consumer --> Dapper
    Consumer --> Cache
    Consumer --> QB
    Consumer --> Page
    Consumer --> IO
    Consumer --> Map
    Consumer --> Audit

    Mongo --> MongoDB
    Dapper --> SQLDB
    Cache --> Redis
    IO --> Files
```

---

## 🏛️ 2. Modelo C4 - Diagramas de Arquitetura

### Nível 1: Diagrama de Contexto de Sistema (C4 Context)

```mermaid
C4Context
    title Diagrama de Contexto de Sistema - DataHelpers

    Person(developer, "Desenvolvedor / Engenheiro .NET", "Consome os pacotes DataHelpers para construir serviços resilientes.")
    System(app, "Aplicações de Negócio", "Web APIs, Microsserviços, Workers em background.")
    System_Boundary(dh_boundary, "Ecossistema DataHelpers") {
        System(datahelpers, "DataHelpers Libraries (8 Módulos)", "Conjunto de pacotes utilitários de acesso a dados, caching, paginação e transformações.")
    }
    SystemDb(relational_dbs, "Bancos Relacionais", "SQL Server, PostgreSQL, MySQL, SQLite, Oracle.")
    SystemDb(document_dbs, "Bancos de Documentos / NoSQL", "MongoDB, DynamoDB.")
    SystemDb(cache_systems, "Sistemas de Cache", "Redis, Memcached, NCache, Apache Ignite, MemoryCache.")
    SystemDb(storage_destinations, "Destinos de Armazenamento", "Sistemas de Arquivos, Serilog, NLog.")

    Rel(developer, app, "Implementa lógica de negócio utilizando")
    Rel(app, datahelpers, "Consome componentes utilitários de")
    Rel(datahelpers, relational_dbs, "Executa comandos e consultas via Dapper / ADO.NET")
    Rel(datahelpers, document_dbs, "Executa operações e agregações via MongoDB.Driver")
    Rel(datahelpers, cache_systems, "Gerencia chaves, regiões e locks distribuídos em")
    Rel(datahelpers, storage_destinations, "Escreve arquivos CSV/Excel/XML e logs de auditoria em")
```

---

### Nível 2: Diagrama de Contêineres (C4 Containers)

```mermaid
C4Container
    title Diagrama de Contêineres - Módulos do DataHelpers (8 Módulos)

    Container_Boundary(c1, "Solução DataHelpers.sln (8 Módulos)") {
        Container(mongo_helper, "TL.MongoDriver.Helpers", "C# / net8.0;net9.0", "Repositórios CQRS, MongoContext transacional, interceptors de eventos e paginação.")
        Container(dapper_helper, "TL.Dapper.Helpers", "C# / net8.0;net9.0", "DapperHelper fluente, Unit of Work transacional ACID, bulk insert e paginação.")
        Container(caching_helper, "TL.Caching.Helpers", "C# / net8.0;net9.0", "Cache hierárquico L1 (Memory) + L2 (Redis), locks distribuídos RedLock e resiliência Polly.")
        Container(query_builder, "TL.QueryBuilder.Helpers", "C# / net8.0;net9.0", "Query builders fluentes para SQL Server, PostgreSQL, MySQL, Oracle, MongoDB e DynamoDB.")
        Container(paging_helper, "TL.PagingFiltering.Helpers", "C# / net8.0;net9.0", "Keyset Seek Pagination O(1), Specification pattern composicional e Dynamic Filter Parser.")
        Container(io_helper, "TL.DataImportExport.Helpers", "C# / net8.0;net9.0", "Importação e exportação assíncrona streaming para CSV, Excel, JSON, XML e ColumnMap.")
        Container(map_helper, "TL.DataMapping", "C# / net8.0;net9.0", "SimpleMapper baseado em árvores de expressões compiladas com Result Pattern.")
        Container(audit_logger, "TL.AuditLogger", "C# / net8.0;net9.0", "Trilhas de auditoria para operações CRUD com diff JSON automático e storage pluggável.")
    }

    Rel(mongo_helper, mongo_helper, "Publica eventos via IEventCatcher")
    Rel(caching_helper, caching_helper, "Comprime payloads via GZip e gerencia regiões de cache")
```

---

## 📦 3. Catálogo Detalhado de Módulos

### 3.1. `MongoDriver.Helpers` (`TL.MongoDriver.Helpers`)
- **Papel:** Abstração completa sobre o driver oficial do MongoDB (`MongoDB.Driver 3.0.0`).
- **Principais Componentes:**
  - `MongoContext` (`IMongoContext`): Gerencia sessões transacionais (`IClientSessionHandle`), enfileiramento de comandos e persistência atômica via `SaveChanges()`.
  - `MongoCommandRepository<T>` / `MongoQueryRepository<T>`: Segregação explícita entre operações de escrita e leitura (CQRS).
  - `MultiCollectionSearchHelper<T>`: Execução de consultas, buscas em texto e agregações cruzadas entre múltiplas coleções.
  - `CaptureEventsInterceptor` / `SaveChangesInterceptor`: Interceptores para disparo de eventos de domínio antes/após commits.
  - `EventCatcher`, `EventHandler`, `Raiser`: Infraestrutura para vinculação dinâmica de handlers e eventos baseados em `IEvent`.
- **Injeção de Dependência:** `services.AddMongoDbContext<TContext>(configuration)`.

### 3.2. `Dapper.Helpers` (`TL.Dapper.Helpers`)
- **Papel:** Facilita a execução de operações SQL, mapeamento objeto-relacional e paginação via Dapper (`Dapper 2.1.35`).
- **Principais Componentes:**
  - `DapperHelper` (`IDapperHelper`): Encapsula execuções escalares, comandos assíncronos e queries tipadas.
  - `IUnitOfWork`: Gerenciamento transacional ACID multi-repositório com rollback automático em caso de falha.
  - `DapperCommandRepository<T>`: Repositório genérico com geração automática de comandos `INSERT`, `UPDATE`, `DELETE` e queries paginadas.
  - `BulkInsertAsync`: Inserção em lotes de alta performance via `SqlBulkCopy` / `Microsoft.Data.SqlClient`.
  - `GenerateWhereClause`: Tradução de árvores de expressão LINQ (`Expression<Func<T, bool>>`) para cláusulas SQL `WHERE`.

### 3.3. `Caching.Helpers` (`TL.Caching.Helpers`)
- **Papel:** Sistema de cache de alta performance com suporte a múltiplos provedores e estratégia multinível.
- **Principais Componentes:**
  - `HierarchicalCacheService` (`IHierarchicalCacheService`): Cache em 2 camadas: L1 local em memória (`IMemoryCache`) com fallback para L2 distribuído (`ICacheService`), incluindo métricas de hit/miss.
  - `RedisCacheService`: Implementação para Redis com compressão GZip automática (`CompressionHelper`) e invalidação por regiões/tenants.
  - `LockedCacheService`: Decorator que implementa Distributed Locking via `RedLock.net` para prevenir condições de corrida (*cache stampede* via Double-Checked Locking).
  - `ThrottledCacheService`: Decorator com controle de vazão e resiliência via `Polly`.
  - Adapters: `MemcachedCacheService`, `NCacheService`, `IgniteCacheService`, `SQLiteCacheService`.

### 3.4. `QueryBuilder.Helpers` (`TL.QueryBuilder.Helpers`)
- **Papel:** Construtor fluente de consultas SQL e NoSQL polimórfico e extensível.
- **Implementações Específicas:**
  - `SQLServerQueryBuilder`, `PostgreSQLQueryBuilder`, `MySQLQueryBuilder`, `OracleQueryBuilder`.
  - `MongoDBQueryBuilder`: Geração de queries e pipelines de agregação BSON.
  - `DynamoDBQueryBuilder`: Expressões de consulta para AWS DynamoDB.
- **Capacidades:** `Select`, `From`, `Where`, `And`, `Or`, `OrderBy`, `Limit`, `Offset`, `Join` (Inner, Left, Right, Full), `GroupBy`, `Having`, `Distinct`, `SubQuery`, `Union`, `UnionAll`, Window Functions (`WithRowNumber`, `WithRank`).

### 3.5. `PagingFiltering.Helpers` (`TL.PagingFiltering.Helpers`)
- **Papel:** Paginação flexível e filtragem avançada baseada em especificações.
- **Principais Componentes:**
  - **Keyset Seek Pagination:** `ApplyKeyset(...)` e `PagedResultKeyset<T, TKey>` com tempo constante O(1) para feeds e grandes volumes.
  - **Specification Pattern:** `ISpecification<T>`, `Specification<T>`, `AndSpecification<T>`, `OrSpecification<T>`, `NotSpecification<T>` com `ParameterReplacer`.
  - `DynamicFilterParser`: Parser de critérios dinâmicos (`FilterCriterion`) para árvores de expressão tipadas.
  - `PaginationHelper<T>` / `PaginationFilterHelper<T>`: Paginação baseada em offset (`PagedResult<T>`) e por cursor (`PagedResultCursor<T>`).

### 3.6. `DataImportExport.Helpers` (`TL.DataImportExport.Helpers`)
- **Papel:** Mecanismo unificado de importação e exportação de dados tabulares e estruturados.
- **Formatos Suportados:**
  - **CSV:** `CsvDataImporter` e `CsvDataExporter` utilizando `CsvHelper 33.0.1`.
  - **Excel:** `ExcelDataImporter` e `ExcelDataExporter` utilizando `ClosedXML 0.104.1`.
  - **JSON:** `JsonDataImporter` e `JsonDataExporter` com `System.Text.Json`.
  - **XML:** `XmlDataImporter` e `XmlDataExporter` nativo via streaming com suporte a namespaces.
  - `ColumnMap<T>`: Mapeador fluente para correspondência de colunas heterogêneas.

### 3.7. `DataMapping.Helpers` (`TL.DataMapping`)
- **Papel:** Mapeador de objetos ultrarrápido em memória com Result Pattern.
- **Funcionamento:** Compila dinamicamente árvores de expressões (`Expression.MemberInit`, `Expression.Lambda`) para realizar binding direto de propriedades públicas sem custo contínuo de reflexão, com cache thread-safe em `ConcurrentDictionary`.
- **Diferenciais:** `TryMap<TSource, TDestination>` retornando `Result<TDestination>`, suporte a coleções aninhadas e conversores customizados (`ITypeConverter<TSource, TDestination>`).

### 3.8. `AuditLogger` (`TL.AuditLogger`)
- **Papel:** Rastreamento e persistência de auditoria de operações (`CREATE`, `READ`, `UPDATE`, `DELETE`).
- **Principais Componentes:**
  - `AuditLogger` (`IAuditLogger`): Registra entradas de auditoria (`AuditLogEntry`) capturando usuário, entidade, timestamps e cálculo automático de diferencial JSON por propriedade (`Old`, `New`, `Diff`).
  - `AuditLogStorageFactory`: Criação sob demanda do armazenamento configurado (`SerilogAuditLogStorage`, `NLogAuditLogStorage`, `MemoryCacheAuditLogStorage`).

---

## ⚙️ 4. Diretrizes de Engenharia e Boas Práticas

1. **Parâmetros de Entrada e Fail-Fast:** Todo método público valida argumentos de entrada via Guard Clauses (`ArgumentNullException.ThrowIfNull(argumento)`).
2. **Eliminação de Falhas Silenciosas:** Proibição estrita de engolir exceções ou retornar estados inconsistentes. Uso extensivo do Result Pattern (`Result<T>`) para tratamento explícito de erros.
3. **Multi-Targeting Rígido:** Compatibilidade garantida simultaneamente para `.NET 8.0` (LTS) e `.NET 9.0` (STS), com política de Zero Warnings (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
4. **Isolamento Total entre Pacotes:** Zero dependências de projeto cruzadas (`ProjectReference`) entre os 8 módulos de produção, garantindo autonomia máxima para aplicações consumidoras.
5. **Async/Await Ubíquo:** Todas as operações de I/O (disco, banco de dados, cache e rede) utilizam interfaces assíncronas com suporte a `CancellationToken`.

---

## 🔗 5. Integração e Relação com Outros Ecossistemas

- **Ecossistema TL.ResilientCore:** Consome os componentes do DataHelpers (como `MongoDriver.Helpers`, `Caching.Helpers` e `QueryBuilder.Helpers`) como blocos de persistência e caching nos microsserviços corporativos.
- **Ecossistema CommonHelpers:** Complementa os utilitários de Health Checks e envelopes `RequestResponse` com capacidades especializadas de persistência e manipulação de dados.
- **Ecossistema TL.MiddlewareLibrary:** Atua em conjunto com o `Caching.Helpers` para estratégias de caching HTTP e rate limiting no pipeline ASP.NET Core.
- **Ecossistema TL.UtilExtensions (ExtensionLibrary):** Fornece métodos de extensão utilitários de alta performance sobre tipos primitivos, coleções e reflexão para enriquecer as manipulações internas.

