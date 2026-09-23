# 📑 TL.PagingFiltering.Helpers

[![NuGet](https://img.shields.io/nuget/v/TL.PagingFiltering.Helpers.svg?style=flat-square&label=TL.PagingFiltering.Helpers)](https://www.nuget.org/packages/TL.PagingFiltering.Helpers/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **Advanced pagination and dynamic filtering library for .NET: Keyset seek pagination O(1), combinable Specification pattern, and dynamic criteria parser.**  
> *Biblioteca avançada de paginação e filtros dinâmicos para .NET: Keyset seek pagination O(1), Specification pattern combinatório e parser dinâmico de critérios.*

O **`TL.PagingFiltering.Helpers`** resolve problemas críticos de latência e consumo de recursos em listagens de grandes volumes. Oferece **Keyset Pagination (Seek Method)** com tempo de resposta constante \(O(1)\), composição elegante de filtros via **Specification Pattern** com normalização de parâmetros (`ParameterReplacer`) e parser de filtros dinâmicos.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.PagingFiltering.Helpers
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Keyset Seek Composto** | `KeysetSeekBuilder<T>`, `ApplyKeysetComposite()` | Construtor fluente para busca baseada em chaves com múltiplas colunas e ordenação mista (ex: Data DESC, Id ASC), com validação defensiva `NOT NULL`. |
| **Keyset Seek Simples** | `ApplyKeyset()`, `PagedResultKeyset<T, TKey>` | Navegação contínua baseada na chave do último elemento, eliminando a degradação de `OFFSET` em tabelas volumosas. |
| **Proteção de Propriedades** | `[FilterIgnore]` | Atributo declarativo em propriedades sensíveis que bloqueia filtros indevidos via `DynamicFilterParser` com `SecurityException`. |
| **Specification Pattern** | `Specification<T>`, `And()`, `Or()`, `Not()` | Composição fluente de regras de negócio em expressões LINQ, utilizando `ParameterReplacer` para consistência. |
| **Parser Dinâmico** | `DynamicFilterParser`, `FilterCriterion` | Mapeamento tipado de filtros de requisição HTTP (Equals, Contains, GreaterThan) para expressões C#. |
| **Paginação por Cursor** | `PagedResultCursor<T>` | Formatação padronizada de envelopes de resposta com cursores anterior e próximo para APIs REST. |
| **Paginação Tradicional** | `PaginationHelper<T>`, `PagedResult<T>` | Suporte a paginação por número de página e tamanho para cenários com totalização obrigatória. |

---

## 💡 Exemplos de Uso

### 1. Paginação Baseada em Chaves (Keyset Seek Composto)

```csharp
using PagingFiltering.Helpers.Builders;
using PagingFiltering.Helpers.Extensions;

IQueryable<Pedido> pedidosQuery = dbContext.Pedidos.AsQueryable();

// Ordenação mista composta: DataCriacao DESC, Id ASC
var seekBuilder = new KeysetSeekBuilder<Pedido>()
    .OrderBy(p => p.DataCriacao, ascending: false)
    .ThenBy(p => p.Id, ascending: true);

var pagina = pedidosQuery.ApplyKeysetComposite(
    seekBuilder,
    lastSeenValues: new object[] { ultimoTimestampVisto, ultimoIdVisto },
    pageSize: 20
);
```

### 2. Blindagem de Propriedades com `[FilterIgnore]`

```csharp
using PagingFiltering.Helpers.Attributes;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    [FilterIgnore]
    public string SenhaHash { get; set; } = string.Empty; // Bloqueado contra filtros dinâmicos externos
}
```

### 2. Composição Fluente com Specification Pattern

```csharp
using PagingFiltering.Helpers.Specifications;

var ativoSpec = new Specification<Usuario>(u => u.Ativo);
var adminSpec = new Specification<Usuario>(u => u.Role == "Administrador");

// Combinação booleana limpa e reutilizável
var filtroFinal = ativoSpec.And(adminSpec);

var expressaoCompilada = filtroFinal.ToExpression();
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre performance de indexação, reescrita de árvores de expressão e comparativos de benchmark:
- 📄 [ADR-005: Decisões Arquiteturais do TL.PagingFiltering.Helpers](../docs/adr/ADR-005-pacote-pagingfiltering-helpers.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).