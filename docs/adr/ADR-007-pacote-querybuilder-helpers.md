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

### 3. Aggregation Pipeline e Filtros NoSQL em BCL Pura (v0.4.0)
- Expurgo total das dependências externas `MongoDB.Driver` e `MongoDB.Bson` do pacote `TL.QueryBuilder.Helpers`.
- O `MongoDBQueryBuilder` foi reescrito para construir filtros e pipelines como JSON puro e válido utilizando exclusivamente `System.Text.Json` nativo da BCL, eliminando transitividade pesada e mantendo independência arquitetural.

### 4. Validação Rigorosa de Identificadores e Escape Dialetal Anti-SQLi (v0.4.0)
- Implementação de `SqlIdentifierValidator`:
  - Validação contra injeções (`^[a-zA-Z_][a-zA-Z0-9_.]*$`) e rejeição de sequências perigosas (`;`, `--`, `/*`, etc.) com `SecurityException`.
  - Escape automático por dialeto: SQL Server `[col]`, PostgreSQL `"col"`, MySQL `` `col` `` e Oracle `"col"`.

### 5. Proteção contra Sobrecarga em Cláusulas LIKE (v0.4.0)
- Adição do método `WhereLike` em `QueryBuilderBase` com escape defensivo de caracteres de coringa (`%`, `_`, `[`).
- Neutraliza cenários em que parâmetros com caracteres coringa arbitrários forçam varreduras completas de tabelas (*full table scan*) no banco de dados relacional.

---

## Consequências e Trade-offs

- **Poliglotismo Desacoplado:** Uma única convenção idiomática de código C# para compor consultas relacionais e NoSQL com zero dependências externas de drivers.
- **Segurança Reforçada contra Injeções:** Proteção contra SQL injection via escape dialetal obrigatório e mitigação de varreduras abusivas em consultas LIKE.
- **Trade-off de Abstração:** Não substitui ORMs completos ou motores de migração de esquema; seu objetivo é geração fluente de consultas dinâmicas de alto rendimento.


