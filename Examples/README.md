# 🚀 Projetos de Exemplo — DataHelpers

Esta pasta contém projetos de exemplo demonstrando na prática o consumo e a integração das bibliotecas da suíte **DataHelpers**.

---

## 🌟 Showcase Integrado

Demonstra o uso conjunto dos pacotes em um pipeline corporativo (ingestão CSV, mapeamento com Result Pattern, construção fluente de queries, persistência com Dapper, paginação, cache hierárquico e auditoria):

```bash
dotnet run --project Examples/Example.Showcase/Example.Showcase.csproj
```

---

## 📂 Exemplos por Biblioteca

Projetos para consulta rápida de cada biblioteca isolada:

| Projeto de Exemplo | Pacote Demonstrado | Principais Cenários Demonstrados |
| :--- | :--- | :--- |
| [**`Example.Showcase`**](./Example.Showcase) | Uso Integrado | Pipeline corporativo unindo os recursos das bibliotecas. |
| [**`Example.DataMapping`**](./Example.DataMapping) | `TL.DataMapping` | Mapeamento com Expression Trees, coleções aninhadas, conversores de tipo e Result Pattern (`TryMap`). |
| [**`Example.Caching`**](./Example.Caching) | `TL.Caching.Helpers` | Caching distribuído no Redis, proteção contra Cache Stampede com RedLock e compressão GZip. |
| [**`Example.AuditLogger`**](./Example.AuditLogger) | `TL.AuditLogger` | Registro de operações CRUD com cálculo automático de diff diferencial JSON. |
| [**`Example.Dapper`**](./Example.Dapper) | `TL.Dapper.Helpers` | Consultas assíncronas, Unit of Work transacional ACID e inserção em lote (`BulkInsertAsync`). |
| [**`Example.PagingFiltering`**](./Example.PagingFiltering) | `TL.PagingFiltering.Helpers` | Paginação tradicional vs Keyset Seek Pagination \(O(1)\) e Specification Pattern combinatório. |
| [**`Example.DataImportExport`**](./Example.DataImportExport) | `TL.DataImportExport.Helpers` | Exportação/Importação assíncrona de CSV, planilhas Excel XLSX, JSON, XML e ColumnMap. |
| [**`Example.QueryBuilder`**](./Example.QueryBuilder) | `TL.QueryBuilder.Helpers` | Construção fluente de consultas SQL, Window Functions (`ROW_NUMBER()`) e MongoDB Aggregation Pipeline. |
| [**`Example.MongoDriver`**](./Example.MongoDriver) | `TL.MongoDriver.Helpers` | Repositórios tipados CQRS no MongoDB e paginação de documentos. |

---

## 🏃 Como Executar os Exemplos

Você pode executar qualquer um dos exemplos diretamente a partir da raiz da solução utilizando o comando `dotnet run`:

```bash
# Executar exemplo de DataMapping
dotnet run --project Examples/Example.DataMapping/Example.DataMapping.csproj

# Executar exemplo de Dapper (utiliza SQLite in-memory)
dotnet run --project Examples/Example.Dapper/Example.Dapper.csproj

# Executar exemplo de Caching
dotnet run --project Examples/Example.Caching/Example.Caching.csproj

# Executar exemplo de Importação e Exportação de Arquivos
dotnet run --project Examples/Example.DataImportExport/Example.DataImportExport.csproj

# Executar exemplo de Paging e Filtering
dotnet run --project Examples/Example.PagingFiltering/Example.PagingFiltering.csproj

# Executar exemplo de QueryBuilder
dotnet run --project Examples/Example.QueryBuilder/Example.QueryBuilder.csproj

# Executar exemplo de Auditoria
dotnet run --project Examples/Example.AuditLogger/Example.AuditLogger.csproj

# Executar exemplo de MongoDriver
dotnet run --project Examples/Example.MongoDriver/Example.MongoDriver.csproj
```

---

> 💡 **Dica:** Os exemplos foram projetados para serem didáticos e executáveis de forma independente, servindo como documentação viva e ponto de partida para a integração em seus próprios microsserviços.
