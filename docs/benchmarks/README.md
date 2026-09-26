# 🔬 Matriz de Evidências Numéricas de Benchmarks — TL.DataHelpers

Este diretório armazena as evidências brutas e análises numéricas consolidadas das suítes de benchmark executadas com **BenchmarkDotNet v0.14.0** sob .NET 8 e .NET 9 em modo **Release**.

---

## 📑 Índice de Evidências por Domínio

1. [**Caching Multinível L1/L2 & FinOps**](./caching-l1-l2-benchmarks.md) — Avaliação de ganho de latência e redução de 99.6% de memória alocada no hit de cache L1 (`IMemoryCache`) vs store remoto L2 serializado, além de compressão GZip e deterministic keygen.
2. [**Keyset Seek O(1) & Paging/Filtering**](./keyset-seek-benchmarks.md) — Análise comparativa entre paginação tradicional deep-page `Skip(N)` e Keyset Seek $O(1)$, com avaliação de filtros via Expression Trees vs Reflection.
3. [**DataMapping & Expression Trees**](./datamapping-benchmarks.md) — Comparativo entre Reflection dinâmica e delegates compilados de Árvore de Expressão com 98.9% de redução de alocação no Heap e Result Pattern.

---

## 📊 Tabela Consolidada de Destaques

| Domínio / Módulo | Cenário Crítico | Baseline Convencional | Otimização TL.DataHelpers | Ganho de Alocação / Throughput |
| :--- | :--- | :--- | :--- | :--- |
| **`TL.Caching.Helpers`** | Resolução de Cache Hit | Store L2 Serializado: 183.784 B, 1.45 ms | **L1 In-Memory Fast Hit: 736 B, 324.9 μs** | **99.6% menos memória**, resolução em sub-milissegundo |
| **`TL.PagingFiltering.Helpers`** | Paginação de Alto Volume | Offset `Skip(9500)`: degradação $O(N)$ | **Keyset Seek: busca indexada $O(1)$** | **Tempo constante** independente da profundidade da página |
| **`TL.DataMapping`** | Mapeamento de Propriedades | Reflection: 46.552 B por invocação | **SimpleMapper: 504 B por invocação** | **98.9% menos memória** (Alloc Ratio = 0.01) |
| **`TL.DataImportExport.Helpers`** | Parsing de CSV / Delimitados | `string.Split`: aloca arrays e strings no Heap | **SpanDelimitedParser: 112 B por registro** | **Zero-Allocation** em leitura por span e reaproveitamento |
| **`TL.AuditLogger`** | Trilha de Auditoria | Clonagem completa de entidade: 3.120 B | **LogUpdate (JSON Diff): apenas delta modificado** | **Até 80% menos volume** trafegado e armazenado |
| **`TL.Dapper.Helpers`** | Conexões e Transações | Conexões avulsas: 1.58 KB por query | **DapperUnitOfWork: 1.14 KB (Reúso)** | Eliminação de handshake repetido e retenção no pool |

---

## 🛠️ Metodologia e Reprodutibilidade

Para reproduzir os resultados localmente em modo Release sob ambiente controlado:

```bash
# Execução da suíte completa
dotnet run -c Release --project benchmarks/DataHelpers.Benchmarks

# Execução individual de suíte específica
dotnet run -c Release --project benchmarks/DataHelpers.Benchmarks --framework net8.0 -- --job dry --filter *Caching*
dotnet run -c Release --project benchmarks/DataHelpers.Benchmarks --framework net8.0 -- --job dry --filter *PagingFiltering*
dotnet run -c Release --project benchmarks/DataHelpers.Benchmarks --framework net8.0 -- --job dry --filter *DataMapping*
```

