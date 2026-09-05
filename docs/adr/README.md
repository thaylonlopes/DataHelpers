# 📚 Architecture Decision Records (ADRs) — DataHelpers

Este repositório documenta formalmente as decisões arquiteturais da suíte **DataHelpers**, estruturadas sob os princípios de design de software de Michael Nygard:
* **Decisões Transversais (ADR-000 e ADR-009):** Governança de compilação, multi-targeting (.NET 8 e .NET 9), injeção de dependência e qualidade transversal de código.
* **Decisões por Módulo (ADR-001 a ADR-008):** Decisões técnicas específicas, padrões de design e isolamento de cada biblioteca da suíte.

---

## 🌐 Governança e Decisões Transversais

| ID | Título / Escopo | Status |
| :--- | :--- | :---: |
| [**ADR-000**](./ADR-000-arquitetura-e-convencoes.md) | **Arquitetura, Governança e Convenções Globais da Solução DataHelpers**<br>*(Multi-Targeting net8.0;net9.0, Zero Warnings, Sem NoWarn, SemVer 0.0.1-beta, DI fluente, XML Docs e Result Pattern)* | `Aceito` |
| [**ADR-009**](./ADR-009-governanca-de-compilacao-multi-targeting-e-zero-warnings.md) | **Governança de Compilação, Multi-Targeting e Política de Zero Warnings**<br>*(TargetFrameworks net8.0;net9.0, TreatWarningsAsErrors, GenerateDocumentationFile e isolamento total entre pacotes)* | `Aceito` |

---

## 📦 Decisões Técnicas por Módulo (1 Pacote = 1 ADR)

| ID | Pacote | Título / Foco Técnico | Status |
| :--- | :--- | :--- | :---: |
| [**ADR-001**](./ADR-001-pacote-datamapping.md) | `TL.DataMapping` | **Mapeamento por Expression Trees**, Coleções Aninhadas, `ITypeConverter` e Result Pattern | `Aceito` |
| [**ADR-002**](./ADR-002-pacote-caching-helpers.md) | `TL.Caching.Helpers` | **Resiliência contra Cache Stampede** com RedLock/Double-Checked Locking, GZip e Invalidação Regional | `Aceito` |
| [**ADR-003**](./ADR-003-pacote-auditlogger.md) | `TL.AuditLogger` | **Cálculo Automático de Diff Estruturado JSON** e Storages Pluggáveis (Memory, Serilog, NLog) | `Aceito` |
| [**ADR-004**](./ADR-004-pacote-dapper-helpers.md) | `TL.Dapper.Helpers` | **Unit of Work Transacional (`IUnitOfWork`)**, `BulkInsertAsync` em lotes e Microsoft.Data.SqlClient | `Aceito` |
| [**ADR-005**](./ADR-005-pacote-pagingfiltering-helpers.md) | `TL.PagingFiltering.Helpers` | **Keyset Pagination Seek (O(1))**, Specification Pattern com ParameterReplacer e Dynamic Filter Parser | `Aceito` |
| [**ADR-006**](./ADR-006-pacote-dataimportexport-helpers.md) | `TL.DataImportExport.Helpers` | **Streaming XML**, Mapeamento Fluente de Colunas (`ColumnMap<T>`) e Integrações CSV/Excel/JSON | `Aceito` |
| [**ADR-007**](./ADR-007-pacote-querybuilder-helpers.md) | `TL.QueryBuilder.Helpers` | **Fluent Query Builder Multi-Dialeto**, Window Functions SQL e MongoDB Aggregation Pipeline | `Aceito` |
| [**ADR-008**](./ADR-008-pacote-mongodriver-helpers.md) | `TL.MongoDriver.Helpers` | **Repositórios CQRS Tipados**, Paginação Assíncrona e MongoDB Driver Moderno | `Aceito` |
