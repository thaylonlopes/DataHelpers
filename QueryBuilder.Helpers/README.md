# 🔍 TL.QueryBuilder.Helpers

[![NuGet](https://img.shields.io/nuget/v/TL.QueryBuilder.Helpers.svg?style=flat-square&label=TL.QueryBuilder.Helpers)](https://www.nuget.org/packages/TL.QueryBuilder.Helpers/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **Fluent, polyglot query builder for .NET: SQL Server, PostgreSQL, MySQL, Oracle, MongoDB BSON pipelines, and Window Functions.**  
> *Construtor fluente e poliglota de consultas para .NET: SQL Server, PostgreSQL, MySQL, Oracle, pipelines BSON no MongoDB e Window Functions.*

O **`TL.QueryBuilder.Helpers`** fornece uma API fluente, tipada e unificada para montagem dinâmica de consultas estruturadas em múltiplos motores relacionais e NoSQL (SQL Server, PostgreSQL, MySQL, Oracle, MongoDB e DynamoDB), incorporando recursos analíticos avançados como *Window Functions* (`ROW_NUMBER()`, `RANK()`).

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.QueryBuilder.Helpers
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Segurança Anti-SQLi** | `SqlIdentifierValidator`, `ValidateAndEscape()` | Validação estrita de identificadores SQL contra injeções (`^[a-zA-Z_][a-zA-Z0-9_.]*$`) e escape por dialeto (SQL Server `[]`, Postgres `""`, MySQL ``` `` ```). |
| **Sanitização de Like** | `WhereLike()` | Sanitização automática de caracteres coringa (`%`, `_`, `[`) prevenindo varreduras completas desnecessárias da tabela. |
| **Dialetos Relacionais** | `SQLServerQueryBuilder`, `PostgreSQLQueryBuilder`, `MySQLQueryBuilder`, `OracleQueryBuilder` | Construtor fluente parametrizado com suporte a `Select`, `From`, `Join`, `Where`, `GroupBy`, `Having` e `OrderBy`. |
| **Window Functions Analíticas** | `WithRowNumber()`, `WithRank()` | Geração padronizada de funções analíticas `OVER (PARTITION BY ... ORDER BY ...)` para numeração e particionamento. |
| **NoSQL BCL Pura** | `MongoDBQueryBuilder`, `DynamoDBQueryBuilder` | Composição de filtros JSON para MongoDB e DynamoDB utilizando exclusivamente `System.Text.Json` da BCL, sem dependências de drivers externos. |
| **Injeção de Dependência** | `AddQueryBuilders()` | Registro de todos os builders polimórficos no container do .NET. |

---

## 💡 Exemplos de Uso

### 1. Construção Fluente SQL com WhereLike e Escape Anti-SQLi

```csharp
using QueryBuilder.Helpers.PostgreSQL;

var builder = new PostgreSQLQueryBuilder();

string sql = builder
    .Select("Id", "Nome", "Email")
    .From("Usuarios")
    .WhereLike("Nome", "Silva", prefixWildcard: true, suffixWildcard: true) // Escapa '%' e '_' automaticamente
    .Where("Ativo = true")
    .Build();
```

### 2. Filtro JSON NoSQL em BCL Pura (MongoDB)

```csharp
using QueryBuilder.Helpers.MongoDB;

var mongoBuilder = new MongoDBQueryBuilder();

string filtroJson = mongoBuilder
    .Where("Status", "Aprovado")
    .WhereGreaterThan("ValorTotal", 1000)
    .Build();
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre suporte a múltiplos dialetos, sanitização de identificadores e performance:
- 📄 [ADR-007: Decisões Arquiteturais do TL.QueryBuilder.Helpers](../docs/adr/ADR-007-pacote-querybuilder-helpers.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).