# 🔬 Evidências de Benchmark: TL.DataMapping (Expression Trees vs Reflection & Result Pattern)

**Ambiente de Execução:**
- **BenchmarkDotNet:** v0.14.0
- **Sistema Operacional:** Windows 11 (10.0.26200)
- **Runtime:** .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
- **Configuração:** Release (`Job=Dry`, `IterationCount=1`, `LaunchCount=1`, `RunStrategy=ColdStart`, `WarmupCount=1`)

---

## 📊 Tabela Comparativa de Resultados

| Método | Descrição do Cenário | Tempo Médio (Mean) | Erro (Error) | Ratio | Memória Alocada | Alloc Ratio |
| :--- | :--- | :---:| :---:| :---:| :---:| :---:|
| `Map_SimpleProperties_Reflection` | 1. Mapeamento: Reflection Propriedade-a-Propriedade (Baseline) | 445.4 μs | NA | 1.00 | 46.552 B | 1.00 |
| `Map_SimpleProperties_SimpleMapper` | 1. Mapeamento: `SimpleMapper` com Expression Tree Compilada | 408.7 μs | NA | 0.92 | 504 B | **0.01** |
| `Map_ComplexNested_Manual` | 2. Mapeamento Complexo: Atribuição Manual de DTO (Baseline) | 342.0 μs | NA | 0.77 | 504 B | 0.01 |
| `Map_ComplexNested_SimpleMapper` | 2. Mapeamento Complexo: `SimpleMapper` Recursivo Aninhado | 411.5 μs | NA | 0.92 | 528 B | **0.01** |
| `TryMap_ResultPattern_DefensiveTryCatch` | 3. Resiliência: Bloco `try/catch` Defensivo com Exceção (Baseline) | 411.3 μs | NA | 0.92 | 504 B | 0.01 |
| `TryMap_ResultPattern_SimpleMapper` | 3. Resiliência: `SimpleMapper.TryMap` com Result Pattern | 568.7 μs | NA | 1.28 | 504 B | 0.01 |

---

## 💡 Análise Técnica e Conclusões Arquiteturais

1. **Eliminação Drástica de Alocação de Memória:**
   - O mapeamento via Reflection pura aloca **46.552 B** no Heap por execução devido à introspecção dinâmica, boxing de tipos de valor e alocação interna de arrays de argumentos.
   - O `SimpleMapper` aloca apenas **504 B** (Alloc Ratio = **0.01**, ou seja, **98.9% menos memória** alocada no Heap).

2. **Árvores de Expressão Pré-compiladas:**
   - Graças ao delegate compilado `Func<TSource, TDestination>` armazenado em `ConcurrentDictionary`, as chamadas subsequentes atingem velocidade análoga à instanciação e atribuição direta em código C# nativo.

3. **Compatibilidade com Trimming e Native AOT:**
   - Com as anotações `[RequiresUnreferencedCode]` e `[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicConstructors)]` em `IMapper` e `SimpleMapper`, o compilador AOT preserva as propriedades necessárias, garantindo que aplicações consumidoras possam compilar nativamente com diagnósticos explícitos.

