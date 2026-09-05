# 🔄 TL.DataMapping

[![NuGet](https://img.shields.io/nuget/v/TL.DataMapping.svg?style=flat-square&label=TL.DataMapping)](https://www.nuget.org/packages/TL.DataMapping/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **High-speed object mapper for .NET based on compiled Expression Trees: Result pattern integration, nested collections, and custom type converters.**  
> *Mapeador de objetos ultrarrápido para .NET baseado em Expression Trees compiladas: integração com Result pattern, coleções aninhadas e conversores customizados de tipo.*

O **`TL.DataMapping`** é um mapeador de objetos de alto desempenho projetado para eliminar o overhead de reflexão em tempo de execução. Ao compilar árvores de expressão (`Expression Trees`) diretamente em delegates nativos cacheados em memória, ele entrega velocidade comparável a código manual com uma API limpa e idiomática.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.DataMapping
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Mapeamento Compilado** | `SimpleMapper`, `Map<TSource, TDestination>()` | Compilação dinâmica de delegates lambda de atribuição com cache thread-safe em `ConcurrentDictionary`. |
| **Result Pattern Funcional** | `TryMap<TSource, TDestination>()` | Mapeamento monádico que retorna `Result<TDestination>`, eliminando o lançamento de exceções para fluxos de validação. |
| **Coleções Aninhadas** | `MapCollection<TSource, TDestination>()` | Suporte transparente a listas, enumeráveis e grafos de objetos aninhados. |
| **Conversores Customizados** | `ITypeConverter<TSource, TDestination>` | Extensibilidade para definir transformações customizadas entre tipos incompatíveis. |
| **Injeção de Dependência** | `AddSimpleMapper()` | Registro como serviço Singleton/Scoped facilitando o consumo em microsserviços. |

---

## 💡 Exemplos de Uso

### 1. Injeção de Dependência no `Program.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddSimpleMapper();
```

### 2. Mapeamento Seguro com Result Pattern

```csharp
using DataMapping.Helpers;
using DataMapping.Helpers.Interfaces;

public class ClienteService
{
    private readonly IMapper _mapper;

    public ClienteService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void Processar(Cliente cliente)
    {
        // Mapeamento direto
        ClienteDto dto = _mapper.Map<Cliente, ClienteDto>(cliente);

        // Mapeamento seguro com Result Pattern
        var result = _mapper.TryMap<Cliente, ClienteDto>(cliente);
        if (result.IsSuccess)
        {
            Console.WriteLine($"Sucesso: {result.Value.Nome}");
        }
    }
}

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre benchmarking, delegates dinâmicos e políticas de zero boxing:
- 📄 [ADR-001: Decisões Arquiteturais do TL.DataMapping](../docs/adr/ADR-001-pacote-datamapping.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).