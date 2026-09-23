# ADR 005: Decisões Arquiteturais do Pacote TL.PagingFiltering.Helpers

---

## Contexto

A paginação tradicional baseada em `OFFSET / FETCH NEXT` (ou `LIMIT`) sofre de degradação linear de desempenho à medida que o usuário avança para páginas distantes em bancos de dados relacionais (por exemplo, buscar a página 10.000 força o banco a escanear e descartar 200.000 linhas). Além disso, APIs REST corporativas necessitam de composição flexível e combinável de filtros de negócio sem risco de injeção de código ou construções inseguras de strings.

O pacote `TL.PagingFiltering.Helpers` foi desenvolvido para solucionar a latência de grandes listagens através de paginação baseada em chaves (Keyset Seek Pagination), composição booleana com o Specification Pattern e parsing dinâmico de critérios.

---

## Decisões Arquiteturais

### 1. Paginação Baseada em Chaves (Keyset Seek Pagination)
- Implementa a paginação por busca de chave (`ApplyKeyset(...)`), retornando o envelope tipado `PagedResultKeyset<T, TKey>`.
- Utiliza a chave do último registro retornado (`afterKey`) como predicado de índice (`WHERE Id > @lastId`), garantindo tempo determinístico de resposta baseado em índice, independentemente do volume total de dados da tabela.

### 2. Specification Pattern com Reescrita de Parâmetros
- Permite construir e combinar regras de negócio booleanas desacopladas utilizando `Specification<T>`, `And()`, `Or()` e `Not()`.
- Utiliza internamente um `ParameterReplacer` baseado em `ExpressionVisitor` para unificar os nós de parâmetro nas árvores de expressão combinadas, evitando falhas de execução no compilador LINQ do runtime .NET.

### 3. Parser Dinâmico de Filtros (`DynamicFilterParser`)
- Converte coleções dinâmicas de critérios de requisição HTTP (`FilterCriterion` suportando operadores `Equals`, `GreaterThan`, `LessThan`, `Contains`, `StartsWith`) diretamente em árvores de expressão `Expression<Func<T, bool>>` compiladas e imutáveis.

### 4. Especialização Keyset Seek Composto & Validação NOT NULL (v0.4.0)
- Criação de `KeysetSeekBuilder<T>` permitindo encadeamento fluente de chaves primárias e secundárias (`OrderBy` e `ThenBy`) com direções mistas (ex: `Data DESC, Id ASC`).
- Validação defensiva em tempo de execução: colunas da chave de seek não podem ser anuláveis (`Nullable.GetUnderlyingType != null`), disparando `InvalidOperationException` preventiva para impedir inconsistências e perda de ponteiro em bancos relacionais.

### 5. Expurgo de Infraestrutura de Cache (v0.4.0)
- Expurgo definitivo da interface `ICacheService`, classe `MemoryCacheService` e do método `ApplyPaginationCachedAsync`, desacoplando totalmente a biblioteca de caching e eliminando riscos de colisão de chaves. A biblioteca reassume responsabilidade única (SRP).

### 6. Proteção contra Exposição de Dados com `[FilterIgnore]` (v0.4.0)
- Criação do atributo declarativo `[FilterIgnore]` para anotação em propriedades de entidades que não devem ser consultadas dinamicamente via requisições HTTP externas.
- O `DynamicFilterParser` rejeita requisições que tentem filtrar ou projetar campos anotados com `SecurityException`, impedindo vazamento de dados confidenciais (ex: senhas, hashes, dados internos).

---

## Consequências e Trade-offs

- **Escalabilidade em Grandes Volumes:** Listagens com milhões de linhas com tempo de resposta consistente via busca por índice (keyset seek) simples ou composto.
- **Segurança de Consulta:** Zero risco de injeção SQL e proteção contra enumeração indevida de atributos internos (`[FilterIgnore]`).
- **Desacoplamento Limpo:** Zero dependências transitivas de caching ou provedores externos de terceiros.
- **Trade-off de Navegação:** Keyset pagination não permite saltar diretamente para páginas aleatórias arbitrárias (ex: "ir para página 47"), sendo ideal para navegação contínua, feeds e scrolls infinitos. Para páginas arbitrárias, o pacote preserva o utilitário clássico de paginação por offset.


