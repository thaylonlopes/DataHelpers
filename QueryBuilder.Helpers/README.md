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
| **Dialetos Relacionais** | `SQLServerQueryBuilder`, `PostgreSQLQueryBuilder`, `MySQLQueryBuilder`, `OracleQueryBuilder` | Construtor fluente parametrizado com suporte a `Select`, `From`, `Join`, `Where`, `GroupBy`, `Having` e `OrderBy`. |
| **Window Functions Analíticas** | `WithRowNumber()`, `WithRank()` | Geração padronizada de funções analíticas `OVER (PARTITION BY ... ORDER BY ...)` para numeração e particionamento. |
| **MongoDB Aggregation** | `MongoDBQueryBuilder` | Composição declarativa de filtros BSON e estágios de agregação para coleções do MongoDB. |
| **Integração NoSQL DynamoDB** | `DynamoDBQueryBuilder` | Construção fluente de expressões de consulta e atributos para DynamoDB. |
| **Injeção de Dependência** | `AddQueryBuilders()` | Registro de todos os builders polimórficos no container do .NET. |

---

## 💡 Exemplos de Uso

### 1. Construção Fluente SQL com Window Function

```csharp
using QueryBuilder.Helpers.SQLServer;

var builder = new SQLServerQueryBuilder();

string sql = builder
    .Select("Id", "Nome", "Preco")
    .WithRowNumber("CategoriaId", "Preco DESC", "PosicaoRank")
    .From("Produtos")
    .Where("Ativo = 1")
    .Where("Preco > 100")
    .OrderBy("Preco", ascending: false)
    .Build();

// Produz a consulta SQL com parâmetros limpos e proteção contra injeção
```

### 2. Filtro BSON para MongoDB

```csharp
using QueryBuilder.Helpers.MongoDB;

var mongoBuilder = new MongoDBQueryBuilder();

var filtroBson = mongoBuilder
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