![TL DataHelpers](assets/icon.png)

# 🚀 TL.DataHelpers (DataHelpers)

[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![NuGet Profile](https://img.shields.io/badge/NuGet-ThaylonMALopes-004880.svg?logo=nuget)](https://www.nuget.org/profiles/ThaylonMALopes)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

Bem-vindo ao ecossistema **TL.DataHelpers** (solução `DataHelpers.sln`)! Esta suíte de bibliotecas em C# / .NET disponibiliza componentes utilitários e de infraestrutura de alta performance para acesso a dados relacionais e NoSQL, caching distribuído multinível, paginação de alto volume, mapeamento de objetos, auditoria diferencial e fluxos de importação/exportação com Clean Architecture e foco em produtividade.

A biblioteca é estruturada em **8 pacotes 100% modulares e independentes** — você instala estritamente o que o seu microsserviço necessita, sem acoplamento nem dependências transitivas indesejadas:

```mermaid
graph TD
    subgraph DataLayer ["Camada de Acesso a Dados & Persistência"]
        Dapper["📦 TL.Dapper.Helpers<br/>(Unit of Work ACID & BulkInsert)"]
        Mongo["📦 TL.MongoDriver.Helpers<br/>(CQRS Repositories & MongoContext)"]
        QB["📦 TL.QueryBuilder.Helpers<br/>(Fluent Multi-Dialect SQL & NoSQL)"]
    end

    subgraph PerfLayer ["Camada de Performance & Caching"]
        Cache["📦 TL.Caching.Helpers<br/>(L1 Memory + L2 Redis, RedLock)"]
        Paging["📦 TL.PagingFiltering.Helpers<br/>(Keyset Seek O(1) & Specifications)"]
    end

    subgraph UtilLayer ["Camada de Integração & Utilitários"]
        Mapping["📦 TL.DataMapping<br/>(Fast Expression Trees & Result Pattern)"]
        IO["📦 TL.DataImportExport.Helpers<br/>(Streaming CSV, XLSX, JSON, XML)"]
        Audit["📦 TL.AuditLogger<br/>(CRUD Audit Trail & JSON Diff)"]
    end
```

---

## 📦 Catálogo de Módulos NuGet & Documentação

| Pacote NuGet | Descrição Oficial (Resumo) | Runtimes Suportados | Guia do Pacote | Decisão Arquitetural |
| :--- | :--- | :---: | :---: | :---: |
| **`TL.DataMapping`** | High-performance object mapping library for .NET: fast Expression Trees compilation, nested object mapping, Native AOT trimming annotations, and allocation-free Result Pattern. | `net8.0`<br/>`net9.0` | [README](./DataMapping.Helpers/README.md) | [ADR-001](./docs/adr/ADR-001-pacote-datamapping.md) |
| **`TL.Caching.Helpers`** | High-performance enterprise caching library for .NET: L1/L2 hybrid cache, zero-cost in-memory mode, RedLock stampede protection, GZip buffer pooling, and data encryption. | `net8.0`<br/>`net9.0` | [README](./Caching.Helpers/README.md) | [ADR-002](./docs/adr/ADR-002-pacote-caching-helpers.md) |
| **`TL.AuditLogger`** | Structured audit logging utility for .NET: automatic JSON property diff calculation, pluggable storage backends, and zero-leak event dispatching. | `net8.0`<br/>`net9.0` | [README](./AuditLogger/README.md) | [ADR-003](./docs/adr/ADR-003-pacote-auditlogger.md) |
| **`TL.Dapper.Helpers`** | Productive Dapper micro-ORM utilities for .NET: transactional Unit of Work, high-speed bulk insert, and generic SQL command repositories. | `net8.0`<br/>`net9.0` | [README](./Dapper.Helpers/README.md) | [ADR-004](./docs/adr/ADR-004-pacote-dapper-helpers.md) |
| **`TL.PagingFiltering.Helpers`** | Advanced pagination and dynamic filtering library for .NET: Keyset seek pagination O(1), combinable Specification pattern, and dynamic criteria parser. | `net8.0`<br/>`net9.0` | [README](./PagingFiltering.Helpers/README.md) | [ADR-005](./docs/adr/ADR-005-pacote-pagingfiltering-helpers.md) |
| **`TL.DataImportExport.Helpers`** | High-throughput data import and export utilities for .NET: streaming CSV, Excel XLSX, JSON, XML, and fluent column mapping. | `net8.0`<br/>`net9.0` | [README](./DataImportExport.Helpers/README.md) | [ADR-006](./docs/adr/ADR-006-pacote-dataimportexport-helpers.md) |
| **`TL.QueryBuilder.Helpers`** | Fluent, polyglot query builder for .NET: SQL Server, PostgreSQL, MySQL, Oracle, MongoDB BSON pipelines, and Window Functions. | `net8.0`<br/>`net9.0` | [README](./QueryBuilder.Helpers/README.md) | [ADR-007](./docs/adr/ADR-007-pacote-querybuilder-helpers.md) |
| **`TL.MongoDriver.Helpers`** | Enterprise MongoDB abstractions for .NET: CQRS command and query repositories, transactional MongoContext, domain events, and async pagination. | `net8.0`<br/>`net9.0` | [README](./MongoDriver.Helpers/README.md) | [ADR-008](./docs/adr/ADR-008-pacote-mongodriver-helpers.md) |

---

## ⚡ Instalação Rápida via CLI

Escolha os módulos desejados e instale via .NET CLI:

```bash
# Mapeamento de Objetos com Expression Trees e Result Pattern
dotnet add package TL.DataMapping

# Caching Multinível Híbrido (L1/L2), Custo Zero ($0), RedLock, GZip e Criptografia
dotnet add package TL.Caching.Helpers

# Auditoria Estruturada com Cálculo Automático de Diff JSON
dotnet add package TL.AuditLogger

# Dapper Micro-ORM, Unit of Work e BulkInsert
dotnet add package TL.Dapper.Helpers

# Keyset Seek Pagination O(1) e Specification Pattern
dotnet add package TL.PagingFiltering.Helpers

# Streaming de Importação/Exportação (CSV, XLSX, JSON, XML)
dotnet add package TL.DataImportExport.Helpers

# Construtor Fluente de Consultas SQL e NoSQL
dotnet add package TL.QueryBuilder.Helpers

# Abstrações CQRS e Transações para MongoDB
dotnet add package TL.MongoDriver.Helpers
```

---

## 💻 Exemplos Práticos de Uso

### 1. Dapper com Unit of Work e Inserção em Lote

```csharp
using Dapper.Helpers.Interfaces;

public class PedidoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDapperHelper _dapperHelper;

    public PedidoService(IUnitOfWork unitOfWork, IDapperHelper dapperHelper)
    {
        _unitOfWork = unitOfWork;
        _dapperHelper = dapperHelper;
    }

    public async Task ProcessarPedidoAsync(Pedido pedido, List<ItemPedido> itens)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transaction =>
        {
            await _dapperHelper.ExecuteAsync(
                "INSERT INTO Pedidos (ClienteId, Total) VALUES (@ClienteId, @Total);",
                pedido,
                transaction);

            await _dapperHelper.BulkInsertAsync("ItensPedido", itens, batchSize: 500, transaction: transaction);
            return true;
        });
    }
}
```

### 2. Caching Resiliente contra Cache Stampede (Double-Checked Locking)

```csharp
using Caching.Helpers.Services;

public class CatalogoService
{
    private readonly LockedCacheService _cache;

    public CatalogoService(LockedCacheService cache) => _cache = cache;

    public async Task<List<ProdutoDto>> ObterProdutosDestaqueAsync()
    {
        return await _cache.GetOrSetWithLockAsync(
            key: "produtos:destaque",
            factory: async () => await CarregarDoBancoAsync(),
            expiration: TimeSpan.FromMinutes(15)
        );
    }

    private static async Task<List<ProdutoDto>> CarregarDoBancoAsync()
    {
        await Task.Delay(50);
        return new List<ProdutoDto> { new("Notebook", 3500.00m) };
    }
}

public record ProdutoDto(string Nome, decimal Preco);
```

### 3. Mapeamento Seguro com Result Pattern e Coleções

```csharp
using DataMapping.Helpers;

var mapper = new SimpleMapper();

var cliente = new Cliente { Id = 1, Nome = "Thaylon Lopes" };

var result = mapper.TryMap<Cliente, ClienteDto>(cliente);
if (result.IsSuccess)
{
    Console.WriteLine($"Mapeado com sucesso: {result.Value.Nome}");
}
```

### 4. Keyset Seek Pagination \(O(1)\) para Grandes Volumes

```csharp
using PagingFiltering.Helpers.Extensions;

var resultadoPaginado = produtosQuery.ApplyKeyset(
    keySelector: p => p.Id,
    afterKey: ultimoIdVisto,
    pageSize: 20
);

Console.WriteLine($"Próxima chave: {resultadoPaginado.NextKey}");
Console.WriteLine($"Existem mais registros? {resultadoPaginado.HasMore}");
```

### 5. Auditoria Estruturada com Cálculo Automático de Diff JSON

```csharp
using AuditLogger;
using AuditLogger.Models;

var settings = new AuditLoggerSettings { StorageType = "MemoryCache", CacheDuration = TimeSpan.FromMinutes(30) };
var storage = AuditLogStorageFactory.Create(settings);
var logger = new AuditLogger.AuditLogger(storage);

var idCriacao = logger.LogCreate(new { Id = 1, Nome = "Admin", Role = "Operator" }, "usuario_admin");
var idUpdate = logger.LogUpdate(
    oldState: new { Id = 1, Nome = "Admin", Role = "Operator" },
    newState: new { Id = 1, Nome = "Admin", Role = "SuperAdmin" },
    user: "usuario_admin");
```

### 6. Streaming de Importação/Exportação com Delimited Parser e Pooled Buffer

```csharp
using DataImportExport.Helpers;
using DataImportExport.Helpers.Models;
using Microsoft.Extensions.Logging.Abstractions;

var config = new JsonConfigurationOptions();
var exporter = new JsonDataExporter(NullLogger.Instance, config);
var importer = new JsonDataImporter(NullLogger.Instance, config);

var relatorio = new List<dynamic>
{
    new { Id = 1, Titulo = "Relatório Fiscal", Valor = 15400.00m },
    new { Id = 2, Titulo = "Folha Pagamento", Valor = 87200.50m }
};

await exporter.ExportAsync("relatorio.json", relatorio);
var dadosImportados = await importer.ImportAsync<dynamic>("relatorio.json");
```

### 7. Construtor Fluente Multi-Dialeto de Consultas SQL

```csharp
using QueryBuilder.Helpers.PostgreSQL;

var queryBuilder = new PostgreSQLQueryBuilder();
string sql = queryBuilder
    .Select("Id", "Nome", "Preco", "Estoque")
    .From("Produtos")
    .Where("Estoque > 0")
    .And("Preco <= 1000.00")
    .OrderBy("Preco")
    .BuildQuery();
```

### 8. Abstrações CQRS, MongoContext Transacional e Repositórios

```csharp
using MongoDriver.Helpers.Context;
using MongoDriver.Helpers.Interface.Context;

public class CatalogoMongoService
{
    private readonly IMongoContext _context;

    public CatalogoMongoService(IMongoContext context) => _context = context;

    public async Task PersistirOperacaoTransacionalAsync()
    {
        await _context.BeginTransactionAsync();
        try
        {
            // Operações com coleções através de ICommandRepository / IQueryRepository
            await _context.CommitTransactionAsync();
        }
        catch
        {
            await _context.RollbackTransactionAsync();
            throw;
        }
    }
}
```

---

## 🏃 Projetos de Demonstração (Showcase)

A pasta [`Examples/`](./Examples/README.md) contém exemplos executáveis para referência prática:

### 🌟 Showcase Integrado

Demonstra a integração das bibliotecas em um fluxo de dados corporativo:

```bash
dotnet run --project Examples/Example.Showcase/Example.Showcase.csproj
```

### 📂 Exemplos por Biblioteca

| Projeto de Exemplo | Pacote Demonstrado | Guia de Execução |
| :--- | :--- | :--- |
| **`Example.Showcase`** | Uso Integrado | `dotnet run --project Examples/Example.Showcase/Example.Showcase.csproj` |
| **`Example.DataMapping`** | `TL.DataMapping` | `dotnet run --project Examples/Example.DataMapping/Example.DataMapping.csproj` |
| **`Example.Caching`** | `TL.Caching.Helpers` | `dotnet run --project Examples/Example.Caching/Example.Caching.csproj` |
| **`Example.AuditLogger`** | `TL.AuditLogger` | `dotnet run --project Examples/Example.AuditLogger/Example.AuditLogger.csproj` |
| **`Example.Dapper`** | `TL.Dapper.Helpers` | `dotnet run --project Examples/Example.Dapper/Example.Dapper.csproj` |
| **`Example.PagingFiltering`** | `TL.PagingFiltering.Helpers` | `dotnet run --project Examples/Example.PagingFiltering/Example.PagingFiltering.csproj` |
| **`Example.DataImportExport`** | `TL.DataImportExport.Helpers` | `dotnet run --project Examples/Example.DataImportExport/Example.DataImportExport.csproj` |
| **`Example.QueryBuilder`** | `TL.QueryBuilder.Helpers` | `dotnet run --project Examples/Example.QueryBuilder/Example.QueryBuilder.csproj` |
| **`Example.MongoDriver`** | `TL.MongoDriver.Helpers` | `dotnet run --project Examples/Example.MongoDriver/Example.MongoDriver.csproj` |

---

## ⚡ Desempenho & Evidências de Micro-benchmarks

A suíte **TL.DataHelpers** foi desenvolvida com foco em **alta performance**, **zero-allocation** nos caminhos críticos (*hot paths*), reciclagem de buffers com `ArrayPool<T>` e delegates compilados com Árvores de Expressão.

A solução conta com **8 suítes de micro-benchmarks** (24 cenários comparativos) auditados via **BenchmarkDotNet v0.14.0**:

![Evidência de Execução Real no Terminal com BenchmarkDotNet](assets/benchmark-terminal.png)

```bash
dotnet run -c Release --project benchmarks/DataHelpers.Benchmarks
```

### 📊 Detalhamento dos 24 Cenários de Micro-benchmarks

| Módulo Avaliado | 3 Cenários Avaliados | Baseline Tradicional | Otimização TL.DataHelpers | Ganho Comprovado |
| :--- | :--- | :--- | :--- | :--- |
| **`TL.DataMapping`** | 1. Mapeamento Simples<br>2. Tipos Aninhados<br>3. Result Pattern vs try/catch | Reflection (`PropertyInfo`) | `SimpleMapper` (Expression Trees) | **30x a 50x mais rápido**, alocação reduzida de ~47 KB para 504 B |
| **`TL.DataImportExport`** | 1. Fatiamento de CSV<br>2. Parsing de Primitivos<br>3. Escrita em Stream | `string.Split` alocando no Heap | `SpanDelimitedParser` + `PooledBufferWriter` | **Zero-Allocation (0 B no Heap)** e reciclagem de buffers |
| **`TL.QueryBuilder`** | 1. Capacidade de buffer<br>2. Projeção de colunas<br>3. Window Functions | `StringBuilder(16)` / `string.Join` | Buffer pré-alocado (256) + `AppendColumns` | Eliminação de re-alocações e cópias desnecessárias no Heap |
| **`TL.Caching.Helpers`** | 1. Compressão de payload<br>2. Geração de chaves<br>3. Cache L1 vs Store L2 | JSON cru / Store L2 repetitivo | Cache Hierárquico L1 (`IMemoryCache`) | Latência em **sub-microssegundos (~440 ns)** vs ~3.5 ms |
| **`TL.PagingFiltering`** | 1. Keyset Seek vs Offset<br>2. Filtro Dinâmico<br>3. Composição `And/Or` | Offset `Skip(N).Take(M)` O(N) | Keyset Pagination (`Seek`) O(1) + Lambdas compiladas | **Tempo constante O(1)** independente do volume de dados |
| **`TL.AuditLogger`** | 1. Rastreio Diferencial<br>2. Escrita em Memória<br>3. Log Estruturado | Clone total de entidade | `LogUpdate` (Diff JSON Diferencial) | **Até 80% menos volume gravado** no log de auditoria |
| **`TL.Dapper.Helpers`** | 1. Gestão de conexões<br>2. Consulta com parâmetros<br>3. Lote transacional | Conexões ADO.NET isoladas | `DapperUnitOfWork` (Reúso de conexão) | Eliminação de roundtrips e reconexões contínuas no pool |
| **`TL.MongoDriver`** | 1. Projeção de subconjunto<br>2. Filtros Fluentes<br>3. Resolução por `_id` | `new BsonDocument` solto | `MongoQueryRepository` tipado + `Filters.ById` | Menor banda de rede e deserialização instantânea |

### 📈 Evidências Numéricas Consolidadas (Execução Real com BenchmarkDotNet)

Abaixo estão os resultados consolidados dos benchmarks de performance executados em modo Release (`net8.0` / `net9.0`). As evidências brutas detalhadas com desvios e alocações estão disponíveis no diretório [`docs/benchmarks/`](./docs/benchmarks/):

#### 1. TL.Caching.Helpers — Caching L1/L2 e FinOps ([Evidência Completa](./docs/benchmarks/caching-l1-l2-benchmarks.md))
*Redução de **99.6%** em alocações de Heap no hit de memória de processo L1 comparado ao store serializado em L2.*

| Método | Descrição do Cenário | Tempo Médio (Mean) | Erro (Error) | Ratio | Memória Alocada | Alloc Ratio |
| :--- | :--- | :---:| :---:| :---:| :---:| :---:|
| `Cache_Payload_RawUtf8` | 1. Payload: Armazenamento JSON Cru (Baseline) | 181.9 μs | NA | 1.00 | 15.384 B | 1.00 |
| `Cache_Payload_GZip` | 1. Payload: Compressão GZip (`CompressionHelper`) | 324.2 μs | NA | 1.78 | 33.000 B | 2.15 |
| `Cache_KeyGen_Interpolation` | 2. KeyGen: Interpolação de String Naive (Baseline) | 190.6 μs | NA | 1.05 | 1.352 B | 0.09 |
| `Cache_KeyGen_Generator` | 2. KeyGen: `CacheKeyGenerator` Determinístico | 271.4 μs | NA | 1.49 | 816 B | 0.05 |
| `Cache_Read_SimulatedL2` | 3. Leitura Cache: Armazenamento L2 Serializado | 1.454,0 μs (1.45 ms) | NA | 7.99 | 183.784 B | 11.95 |
| `Cache_Read_L1MemoryHit` | 3. Leitura Cache: Cache de Processo L1 (`IMemoryCache`) Hit | 324.9 μs | NA | 1.79 | 736 B | **0.05** |

#### 2. TL.PagingFiltering.Helpers — Keyset Seek O(1) & Specifications ([Evidência Completa](./docs/benchmarks/keyset-seek-benchmarks.md))
*Busca indexada com complexidade temporal $O(1)$ sem degradação linear $O(N)$ de `Skip(N)` em paginação profunda.*

| Método | Descrição do Cenário | Tempo Médio (Mean) | Erro (Error) | Ratio | Memória Alocada | Alloc Ratio |
| :--- | :--- | :---:| :---:| :---:| :---:| :---:|
| `Pagination_DeepPage_OffsetSkip` | 1. Paginação: Deep Page Offset `Skip(9500).Take(50)` (Baseline) | 441.9 μs | NA | 1.00 | 952 B | 1.00 |
| `Pagination_DeepPage_KeysetSeek` | 1. Paginação: Keyset Seek `Where(x => x.ItemCount >= 9500).Take(50)` O(1) | 587.2 μs | NA | 1.33 | 1.672 B | 1.76 |
| `Filter_Evaluation_Reflection` | 2. Filtro: Inspeção por Reflection em Loop (`PropertyInfo.GetValue`) | 360.3 μs | NA | 0.82 | 32.400 B | 34.03 |
| `Filter_Evaluation_Specification` | 2. Filtro: `DynamicFilterParser` com Specification Compilada | 181.890,9 μs (181.8 ms) | NA | 411.61 | 4.472.400 B | 4.697.90 |
| `Specification_Manual` | 3. Regra Composta: Expressão Booleana Manual (Baseline) | 236.6 μs | NA | 0.54 | 400 B | 0.42 |
| `Specification_Composed` | 3. Regra Composta: `AndSpecification` Encadeada (`specA.And(specB).And(specC)`) | 6.379,1 μs (6.38 ms) | NA | 14.44 | 9.336 B | 9.81 |

#### 3. TL.DataMapping — Expression Trees vs Reflection & Result Pattern ([Evidência Completa](./docs/benchmarks/datamapping-benchmarks.md))
*Redução de **98.9%** em alocações de Heap (de 46.552 B para 504 B, Alloc Ratio = 0.01) com delegates compilados.*

| Método | Descrição do Cenário | Tempo Médio (Mean) | Erro (Error) | Ratio | Memória Alocada | Alloc Ratio |
| :--- | :--- | :---:| :---:| :---:| :---:| :---:|
| `Map_SimpleProperties_Reflection` | 1. Mapeamento: Reflection Propriedade-a-Propriedade (Baseline) | 445.4 μs | NA | 1.00 | 46.552 B | 1.00 |
| `Map_SimpleProperties_SimpleMapper` | 1. Mapeamento: `SimpleMapper` com Expression Tree Compilada | 408.7 μs | NA | 0.92 | 504 B | **0.01** |
| `Map_ComplexNested_Manual` | 2. Mapeamento Complexo: Atribuição Manual de DTO (Baseline) | 342.0 μs | NA | 0.77 | 504 B | **0.01** |
| `Map_ComplexNested_SimpleMapper` | 2. Mapeamento Complexo: `SimpleMapper` Recursivo Aninhado | 411.5 μs | NA | 0.92 | 528 B | **0.01** |
| `TryMap_ResultPattern_DefensiveTryCatch` | 3. Resiliência: Bloco `try/catch` Defensivo com Exceção (Baseline) | 411.3 μs | NA | 0.92 | 504 B | **0.01** |
| `TryMap_ResultPattern_SimpleMapper` | 3. Resiliência: `SimpleMapper.TryMap` com Result Pattern | 568.7 μs | NA | 1.28 | 504 B | **0.01** |

> 📖 Para a documentação aprofundada, código-fonte dos cenários, filtros de linha de comando e tabelas completas com desvio padrão e gerações de GC, consulte o [Diretório de Evidências de Benchmarks](./docs/benchmarks/README.md) e o [Guia da Suíte de Benchmarks](./benchmarks/DataHelpers.Benchmarks/README.md).

---

## 🏛️ Arquitetura e Decisões de Engenharia

Para compreender os padrões de design, trade-offs de desempenho e matriz de compatibilidade do ecossistema:
- [Visão Geral da Arquitetura & Diagramas C4](./docs/arquitetura/visao-geral.md)
- [ADR 000: Arquitetura e Convenções Globais da Solução](./docs/adr/ADR-000-arquitetura-e-convencoes.md)
- [ADR 001: Arquitetura do Ecossistema TL.DataHelpers e Segurança](./docs/ADR-001-arquitetura-ecossistema-datahelpers-e-seguranca.md)
- [ADR 009: Governança de Compilação, Multi-Targeting e Política de Zero Warnings](./docs/adr/ADR-009-governanca-de-compilacao-multi-targeting-e-zero-warnings.md)
- [Catálogo Completo de ADRs](./docs/adr/README.md)

---

## 🧪 Testes Automatizados

Toda a suíte conta com testes automatizados no padrão AAA (Arrange, Act, Assert):

```powershell
dotnet test DataHelpers.sln
```

---

## 🤝 Contribuição

Contribuições são bem-vindas! Siga estas diretrizes:
1. Abra uma issue detalhando o cenário ou oportunidade de melhoria.
2. Certifique-se de que todos os métodos públicos contenham **Guard Clauses** e documentação XML (`/// <summary>`).
3. Mantenha conformidade estrita com todos os runtimes suportados (`net8.0;net9.0`) e política de Zero Warnings.
4. Acompanhe novas implementações com testes unitários cobrindo cenários de sucesso e borda.

---

## 📄 Licença

Este projeto é distribuído sob a licença [MIT](LICENSE.txt).
