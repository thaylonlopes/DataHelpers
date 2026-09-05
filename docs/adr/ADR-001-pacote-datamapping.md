# ADR 001: Decisões Arquiteturais do Pacote TL.DataMapping

---

## Contexto

Aplicações .NET corporativas e microsserviços frequentemente necessitam converter dados entre Entidades de Domínio, Data Transfer Objects (DTOs) e ViewModels. Mapeadores tradicionais baseados em reflexão pura (`System.Reflection`) impõem penalidades substanciais de latência e consumo de CPU. Por outro lado, ferramentas complexas com compiladores de mapeamento estáticos frequentemente exigem configurações excessivamente verbosas e perfis acoplados.

O pacote `TL.DataMapping` foi projetado para fornecer um meio-termo de alto desempenho: mapeamento nominal por convenção suportado por compilação dinâmica de árvores de expressão (`Expression Trees`), Result Pattern nativo e zero overhead repetitivo.

---

## Decisões Arquiteturais

### 1. Compilação Dinâmica via Expression Trees com Cache Thread-Safe
- O `SimpleMapper` inspeciona as propriedades públicas na primeira execução e compila delegates tipados `Func<TSource, TDestination>` utilizando `System.Linq.Expressions` (`Expression.MemberInit`, `Expression.New`).
- Os delegates compilados são armazenados em um cache estático thread-safe baseado em `ConcurrentDictionary<(Type, Type), Delegate>`, garantindo que invocações subsequentes atinjam performance equivalente a código estático escrito manualmente.

### 2. Mapeamento Recursivo e Suporte a Coleções Aninhadas
- A compilação detecta automaticamente propriedades complexas e coleções (`IEnumerable<T>`, `List<T>`, arrays).
- O motor gera chamadas internas de mapeamento encadeado para elementos filhos, permitindo converter grafos de objetos profundos sem mapeamento manual declarativo.

### 3. Conversores Customizados de Tipos (`ITypeConverter`)
- Para tipos com regras de conversão não-triviais (por exemplo, `string` para `DateTime`, ou enums customizados para códigos legados), o pacote disponibiliza a interface `ITypeConverter<TSource, TDestination>`.
- Os conversores são registrados diretamente no mapeador e injetados na compilação do delegate.

### 4. Adoção do Result Pattern Funcional (`TryMap`)
- Além do método convencional `Map<TSource, TDestination>(source)`, a API oferece o método monádico `TryMap<TSource, TDestination>(source)`.
- Retorna `Result<TDestination>` com propriedades `IsSuccess`, `Value` e `Error`, evitando lançar exceções de fluxo de controle quando o payload de entrada for inválido ou incompatível.

---

## Consequências e Trade-offs

- **Performance:** Execução ultrarrápida próxima ao código C# manual após a compilação do delegate.
- **Robustez Funcional:** O Result Pattern permite tratamento limpo de erros sem sobrecarga de exceptions na CLR.
- **Trade-off de Inicialização:** A primeira invocação para cada par de tipos incorre em um pequeno custo de compilação da árvore de expressões (mitigado pelo cache concorrente permanente).

