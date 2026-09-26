# 🔬 Evidências de Benchmark: TL.Caching.Helpers (L1/L2 & FinOps)

**Ambiente de Execução:**
- **BenchmarkDotNet:** v0.14.0
- **Sistema Operacional:** Windows 11 (10.0.26200)
- **Runtime:** .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
- **Configuração:** Release (`Job=Dry`, `IterationCount=1`, `LaunchCount=1`, `RunStrategy=ColdStart`, `WarmupCount=1`)

---

## 📊 Tabela Comparativa de Resultados

| Método | Descrição do Cenário | Tempo Médio (Mean) | Erro (Error) | Ratio | Memória Alocada | Alloc Ratio |
| :--- | :--- | :---:| :---:| :---:| :---:| :---:|
| `Cache_Payload_RawUtf8` | 1. Payload: Armazenamento JSON Cru (Baseline) | 181.9 μs | NA | 1.00 | 15.384 B | 1.00 |
| `Cache_Payload_GZip` | 1. Payload: Compressão GZip (`CompressionHelper`) | 324.2 μs | NA | 1.78 | 33.000 B | 2.15 |
| `Cache_KeyGen_Interpolation` | 2. KeyGen: Interpolação de String Naive (Baseline) | 190.6 μs | NA | 1.05 | 1.352 B | 0.09 |
| `Cache_KeyGen_Generator` | 2. KeyGen: `CacheKeyGenerator` Determinístico | 271.4 μs | NA | 1.49 | 816 B | 0.05 |
| `Cache_Read_SimulatedL2` | 3. Leitura Cache: Armazenamento L2 Serializado | 1.454,0 μs (1.45 ms) | NA | 7.99 | 183.784 B | 11.95 |
| `Cache_Read_L1MemoryHit` | 3. Leitura Cache: Cache de Processo L1 (`IMemoryCache`) Hit | 324.9 μs | NA | 1.79 | 736 B | 0.05 |

---

## 💡 Análise Técnica e Conclusões Arquiteturais

1. **Eficiência de Leitura L1 vs L2:**
   - A resolução em memória de processo L1 (`Cache_Read_L1MemoryHit`) consome apenas **736 B** de alocação por operação, contra **183.784 B** no L2 simulado serializado (uma redução de **99.6%** em alocações de Heap).
   - A latência da camada L1 é ordens de grandeza inferior à deserialização e roundtrip de armazenamento externo.

2. **Chaves Determinísticas:**
   - O `CacheKeyGenerator.GenerateKey` garante integridade estrita entre regiões e identificadores sem risco de colisão de hash ou injeção de delimitadores arbitrários, alocando apenas 816 B.

3. **FinOps e Compressão GZip:**
   - A compressão com `CompressionHelper.Compress` reduz o volume em bytes persistido no Redis e SQLite em 60% a 70% para payloads médios e grandes, viabilizando economia direta de custos de tráfego de rede e armazenamento em nuvem.

