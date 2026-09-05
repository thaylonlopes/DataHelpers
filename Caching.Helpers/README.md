# ⚡ TL.Caching.Helpers

[![NuGet](https://img.shields.io/nuget/v/TL.Caching.Helpers.svg?style=flat-square&label=TL.Caching.Helpers)](https://www.nuget.org/packages/TL.Caching.Helpers/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **High-performance multilevel caching library for .NET: L1/L2 hybrid cache, RedLock stampede protection, GZip compression, and tenant partitioning.**  
> *Biblioteca de caching multinível de alta performance para .NET: cache híbrido L1/L2, proteção contra cache stampede com RedLock, compressão GZip e particionamento por tenant.*

O **`TL.Caching.Helpers`** disponibiliza uma camada de cache corporativa completa, combinando cache em memória ultrarrápido (L1) com cache distribuído resiliente no Redis (L2). Conta com mecanismos avançados de *Distributed Locking* (RedLock) para erradicar o problema de *Cache Stampede*, compressão de tráfego e suporte nativo a isolamento multi-tenant.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.Caching.Helpers
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Cache Híbrido Multinível** | `IHierarchicalCacheService` | Consulta em memória (L1); em caso de miss, busca no Redis (L2) e repovoa o L1 de forma transparente. |
| **Proteção Anti-Stampede** | `LockedCacheService` | Implementação de lock distribuído (*Double-Checked Locking*) via RedLock, impedindo que milhares de requisições sobrecarreguem o banco ao expirar uma chave. |
| **Compressão de Dados** | `CompressionHelper` (GZip) | Compactação automática de payloads volumosos antes da gravação no Redis, economizando tráfego de rede e memória. |
| **Particionamento por Regiões** | `region`, `tenantId` | Agrupamento lógico de chaves para invalidação em lote e isolamento seguro de múltiplos clientes. |
| **Controle de Vazão** | `AddThrottledCache()` | Integração fluente com políticas de resiliência e limitação de requisições baseadas em Polly. |

---

## 💡 Exemplos de Uso

### 1. Injeção de Dependência no `Program.cs`

```csharp
using Caching.Helpers.Extensions;

// Registra Redis distribuído
builder.Services.AddRedisCache(builder.Configuration);

// Registra Cache Hierárquico L1 (Memory) + L2 (Redis)
builder.Services.AddHierarchicalCache(builder.Configuration);
```

### 2. Consulta Segura contra Cache Stampede

```csharp
using Caching.Helpers.Services;

public class CatalogoService
{
    private readonly LockedCacheService _cacheService;

    public CatalogoService(LockedCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<ProdutoDto>> ObterProdutosDestaqueAsync()
    {
        return await _cacheService.GetOrSetWithLockAsync(
            key: "produtos:destaque",
            factory: async () => await CarregarDoBancoDeDadosAsync(),
            expiration: TimeSpan.FromMinutes(10)
        );
    }

    private static async Task<List<ProdutoDto>> CarregarDoBancoDeDadosAsync()
    {
        await Task.Delay(50);
        return new List<ProdutoDto>
        {
            new("Notebook Corporativo", 4500.00m),
            new("Monitor UltraWide", 1800.00m)
        };
    }
}

public record ProdutoDto(string Nome, decimal Preco);
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre locking distribuído, arquitetura de camadas e mitigação de latência:
- 📄 [ADR-002: Decisões Arquiteturais do TL.Caching.Helpers](../docs/adr/ADR-002-pacote-caching-helpers.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).