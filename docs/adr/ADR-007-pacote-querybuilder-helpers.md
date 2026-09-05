# ADR 007: Decisões Arquiteturais do Pacote TL.QueryBuilder.Helpers

---

## Contexto

Aplicações corporativas que adotam arquiteturas poliglotas frequentemente operam simultaneamente com bancos relacionais (SQL Server, PostgreSQL, MySQL, Oracle) e bancos NoSQL orientados a documentos (MongoDB) ou chave-valor (AWS DynamoDB). Montar consultas dinâmicas concatenando strings puras é altamente propenso a falhas de sintaxe e vulnerabilidades de SQL Injection.

O pacote `TL.QueryBuilder.Helpers` foi desenvolvido para unificar a composição dinâmica de consultas através de uma Fluent API fortemente estruturada, agregando suporte nativo a comandos analíticos modernos como *Window Functions* e *Aggregation Pipelines*.

---

## Decisões Arquiteturais

### 1. Fluent API Unificada (`IQueryBuilder` e `QueryBuilderBase`)
- Padronização de métodos fluentes para composição estruturada de instruções:
  - `Select`, `From`, `Join` (Inner, Left, Right, Full), `Where`, `And`, `Or`, `GroupBy`, `Having`, `OrderBy`, `Limit`, `Offset`, `Distinct`, `Union` e `SubQuery`.
- Cada dialeto relacional (`SQLServerQueryBuilder`, `PostgreSQLQueryBuilder`, `MySQLQueryBuilder`, `OracleQueryBuilder`) implementa regras sintáticas específicas (ex: uso de colchetes `[]` no SQL Server vs aspas duplas `""` no PostgreSQL vs crases ```` ` ` ```` no MySQL).

### 2. Suporte a Window Functions Analíticas
- Adição dos métodos `WithRowNumber` e `WithRank` na API fluente, permitindo construir expressões analíticas parametrizadas com partições e ordenação:
  ```csharp
  builder.Select("Id", "Nome", "Salario")
         .WithRowNumber("DepartamentoId", "Salario DESC", "SeqSalario");
  ```
- Gera a cláusula padronizada `ROW_NUMBER() OVER (PARTITION BY DepartamentoId ORDER BY Salario DESC) AS SeqSalario`.

### 3. Aggregation Pipeline Fluente no MongoDB
- O `MongoDBQueryBuilder` provê construtores fluentes para composição de filtros BSON e estágios de agregação (`Match`, `Group`, `Facet`, `Sort`), gerando instâncias tipadas para o driver oficial do MongoDB.

---

## Consequências e Trade-offs

- **Poliglotismo Estruturado:** Uma única convenção idiomática de código C# para compor consultas em 6 motores distintos de persistência.
- **Segurança de Execução:** Separação entre cláusulas e parâmetros previne injeção inadvertida de SQL.
- **Trade-off de Abstração:** Não substitui ORMs completos ou motores de migração de esquema; seu objetivo é geração de consultas dinâmicas de alto rendimento.

