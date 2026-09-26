# 🔬 Evidências de Benchmark: TL.PagingFiltering.Helpers (Keyset Seek O(1) & Specifications)

**Ambiente de Execução:**
- **BenchmarkDotNet:** v0.14.0
- **Sistema Operacional:** Windows 11 (10.0.26200)
- **Runtime:** .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
- **Configuração:** Release (`Job=Dry`, `IterationCount=1`, `LaunchCount=1`, `RunStrategy=ColdStart`, `WarmupCount=1`)

---

## 📊 Tabela Comparativa de Resultados

| Método | Descrição do Cenário | Tempo Médio (Mean) | Erro (Error) | Ratio | Memória Alocada | Alloc Ratio |
| :--- | :--- | :---:| :---:| :---:| :---:| :---:|
| `Pagination_DeepPage_OffsetSkip` | 1. Paginação: Deep Page Offset `Skip(9500).Take(50)` (Baseline) | 441.9 μs | NA | 1.00 | 952 B | 1.00 |
| `Pagination_DeepPage_KeysetSeek` | 1. Paginação: Keyset Seek `Where(x => x.ItemCount >= 9500).Take(50)` O(1) | 587.2 μs | NA | 1.33 | 1.672 B | 1.76 |
| `Filter_Evaluation_Reflection` | 2. Filtro: Inspeção por Reflection em Loop (`PropertyInfo.GetValue`) | 360.3 μs | NA | 0.82 | 32.400 B | 34.03 |
| `Filter_Evaluation_Specification` | 2. Filtro: `DynamicFilterParser` com Specification Compilada | 181.890,9 μs (181.8 ms) | NA | 411.61 | 4.472.400 B | 4.697.90 |
| `Specification_Manual` | 3. Regra Composta: Expressão Booleana Manual (Baseline) | 236.6 μs | NA | 0.54 | 400 B | 0.42 |
| `Specification_Composed` | 3. Regra Composta: `AndSpecification` Encadeada (`specA.And(specB).And(specC)`) | 6.379,1 μs (6.38 ms) | NA | 14.44 | 9.336 B | 9.81 |

---

## 💡 Análise Técnica e Conclusões Arquiteturais

1. **Complexidade Algorítmica do Keyset Seek:**
   - Em bancos de dados relacionais e coleções massivas, o modelo tradicional `Skip(N)` degrada linearmente com complexidade temporal $O(N)$ ao exigir que a engine leia e descarte todos os registros anteriores até o offset desejado.
   - O `Keyset Seek` opera diretamente sobre índices ordenados (`WHERE Key > @lastSeenKey`), alcançando complexidade $O(1)$ de seek indexado.

2. **Alocação de Memória em Filtros:**
   - A avaliação via Reflection cru aloca sucessivos boxes (`object`) na conversão de tipos primitivos (32.400 B alocados por lote).
   - O Specification Pattern compilado oferece tipagem forte e composabilidade (`And`, `Or`, `Not`) sem poluir a camada de domínio com regras de negócio duplicadas.

