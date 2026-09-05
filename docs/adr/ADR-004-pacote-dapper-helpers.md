# ADR 004: Decisões Arquiteturais do Pacote TL.Dapper.Helpers

---

## Contexto

O micro-ORM Dapper é referência da indústria em execução veloz de consultas e comandos SQL em .NET. Entretanto, em cenários corporativos complexos, desenvolvedores frequentemente precisam escrever código repetitivo para abrir e fechar conexões, orquestrar transações atômicas ACID que envolvem múltiplos repositórios e realizar cargas massivas de dados sem a lentidão de múltiplos roundtrips de rede.

O pacote `TL.Dapper.Helpers` foi desenvolvido para preencher essa lacuna, fornecendo o padrão Unit of Work transacional, inserção em lote (`BulkInsertAsync`) e repositórios baseados no driver oficial moderno `Microsoft.Data.SqlClient`.

---

## Decisões Arquiteturais

### 1. Padrão Unit of Work Transacional (`IUnitOfWork`)
- Disponibiliza uma abstração robusta para gerenciar o escopo de transações atômicas com rollback automático em caso de exceção:
  - `ExecuteInTransactionAsync(Func<IDbTransaction, Task<TResult>>)`
  - `BeginTransactionAsync()`, `CommitAsync()` e `RollbackAsync()`
- Todas as operações em `IDapperHelper` e repositórios de comando aceitam o parâmetro opcional `IDbTransaction? transaction = null`, garantindo que múltiplas operações de repositório possam participar da mesma transação física.

### 2. Inserção Massiva em Lotes (`BulkInsertAsync<T>`)
- Fornece utilitário de inserção em batch otimizado com tamanho de lote configurável (`batchSize = 1000`).
- Gerencia dinamicamente colunas de identidade (`includeId = false`) e participa de transações abertas de forma transparente.

### 3. Migração Definitiva para `Microsoft.Data.SqlClient`
- Utiliza exclusivamente o provedor oficial moderno `Microsoft.Data.SqlClient` (eliminando o pacote legado `System.Data.SqlClient`), garantindo suporte total a recursos modernos do SQL Server, criptografia Always Encrypted e Azure SQL Database.

---

## Consequências e Trade-offs

- **Garantia ACID:** Atomicidade integral em operações distribuídas entre múltiplos repositórios na mesma conexão.
- **Produtividade de Dados:** Inserções de milhares de registros por segundo sem a necessidade de configurar ferramentas pesadas de ORM.
- **Trade-off de Conexão:** O controle transacional exige que a conexão permaneça aberta durante o bloco transacional, exigindo disciplina para manter os escopos curtos e estritamente necessários.

