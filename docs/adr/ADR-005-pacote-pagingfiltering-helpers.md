# ADR 005: Decisões Arquiteturais do Pacote TL.PagingFiltering.Helpers

---

## Contexto

A paginação tradicional baseada em `OFFSET / FETCH NEXT` (ou `LIMIT`) sofre de degradação linear de desempenho à medida que o usuário avança para páginas distantes em bancos de dados relacionais (por exemplo, buscar a página 10.000 força o banco a escanear e descartar 200.000 linhas). Além disso, APIs REST corporativas necessitam de composição flexível e combinável de filtros de negócio sem risco de injeção de código ou construções inseguras de strings.

O pacote `TL.PagingFiltering.Helpers` foi desenvolvido para solucionar a latência de grandes listagens através de Keyset Seek Pagination \(O(1)\), composição booleana com o Specification Pattern e parsing dinâmico de critérios.

---

## Decisões Arquiteturais

### 1. Keyset Seek Pagination \(O(1)\)
- Implementa a paginação por busca de chave (`ApplyKeyset(...)`), retornando o envelope tipado `PagedResultKeyset<T, TKey>`.
- Utiliza a chave do último registro retornado (`afterKey`) como predicado de índice (`WHERE Id > @lastId`), garantindo tempo constante de resposta \(O(1)\) independentemente do volume de dados da tabela.

### 2. Specification Pattern com Reescrita de Parâmetros
- Permite construir e combinar regras de negócio booleanas desacopladas utilizando `Specification<T>`, `And()`, `Or()` e `Not()`.
- Utiliza internamente um `ParameterReplacer` baseado em `ExpressionVisitor` para unificar os nós de parâmetro nas árvores de expressão combinadas, evitando falhas de execução no compilador LINQ do runtime .NET.

### 3. Parser Dinâmico de Filtros (`DynamicFilterParser`)
- Converte coleções dinâmicas de critérios de requisição HTTP (`FilterCriterion` suportando operadores `Equals`, `GreaterThan`, `LessThan`, `Contains`, `StartsWith`) diretamente em árvores de expressão `Expression<Func<T, bool>>` compiladas e imutáveis.

---

## Consequências e Trade-offs

- **Escalabilidade Extrema:** Listagens com milhões de linhas com tempo de resposta imutável em feeds e tabelas.
- **Segurança de Consulta:** Zero risco de injeção SQL, pois toda a filtragem dinâmica é compilada via Expression Trees tipadas.
- **Trade-off de Navegação:** Keyset pagination não permite saltar diretamente para páginas aleatórias arbitrárias (ex: "ir para página 47"), sendo ideal para navegação contínua, feeds e scrolls infinitos. Para páginas arbitrárias, o pacote preserva o utilitário clássico de paginação por offset.

