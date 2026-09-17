# ⚡ TL.Caching.Helpers

[![NuGet](https://img.shields.io/nuget/v/TL.Caching.Helpers.svg?style=flat-square&label=TL.Caching.Helpers)](https://www.nuget.org/packages/TL.Caching.Helpers/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **High-performance enterprise caching library for .NET: L1/L2 hybrid cache, zero-cost in-memory fallback, RedLock stampede protection, GZip buffer pooling, and data protection encryption.**  
> *Biblioteca corporativa de caching de alta performance para .NET: cache híbrido L1/L2, fallback em memória Custo Zero ($0), proteção contra cache stampede com RedLock, compressão GZip com pooling e criptografia para proteção de dados.*

O **`TL.Caching.Helpers`** disponibiliza uma camada de cache corporativa completa e de alta vazão para aplicações modernas em .NET 8 e .NET 9. Combina cache local em memória (L1) com cache distribuído resiliente no Redis (L2), proteção anti-stampede via *Double-Checked Locking* com RedLock, compressão GZip com reciclagem de memória via `ArrayPool<byte>.Shared`, mitigação de concorrência com *TTL Jitter*, e proteção de dados com criptografia em repouso.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.Caching.Helpers
```

---

## 🚀 Funcionalidades Principais (v0.3.0)

| Funcionalidade | Componentes & Classes | Descrição |
| :--- | :--- | :--- |
| **Modo Custo Zero ($0)** | `MemoryCacheService`, `AddTlCaching()` | Execução 100% em memória local com `IMemoryCache` quando `Cache:UseRedis == false` ou ausente, sem custo de infraestrutura de nuvem. |
| **Compressão GZip com ArrayPool** | `GZipCompressionProvider`, `ICacheCompression` | Compactação automática de payloads maiores que 1 KB com economia de mais de 60% de banda/RAM e bypass inteligente para payloads < 1 KB. |
| **Resiliência de TTL com Jitter** | `TtlJitterCalculator` | Dispersão pseudo-aleatória de expiração em ±10% a ±15%, impedindo que milhares de chaves expirem no mesmo segundo (*Thundering Herd*). |
| **Double-Checked Locking & Fallback** | `CacheLockHelper`, `LockedCacheService` | Lock distribuído restrito a cache miss para recálculo da factory. Leituras e escritas convencionais operam livres de lock, com fallback silencioso caso o Redis caia. |
| **Criptografia e Proteção de Dados** | `AesEncryptionProvider`, `ICacheEncryption` | Proteção de dados confidenciais (PII, tokens) em nível de aplicação com integridade e limpeza de memória (`ZeroMemory`). |
| **Pipeline Ordenado** | `CachePayloadPipeline` | Integração sequencial estrita: Serialização -> Compressão (GZip) -> Criptografia. |
| **Particionamento por Regiões** | `region`, `InvalidateRegionAsync` | Invalidação atômica de chaves agrupadas por região ou tenant sem comandos bloqueantes globais. |

---

## 💡 Exemplos de Configuração e Uso

### 1. Configuração Declarativa no `appsettings.json`

#### Modo Custo Zero ($0) — Execução Local / Testes
```json
{
  "Cache": {
    "UseRedis": false
  }
}
```

#### Modo Produção Distribuído com Redis
```json
{
  "Cache": {
    "UseRedis": true,
    "RedisConnectionString": "redis-cluster.internal:6379,ssl=true,abortConnect=false"
  }
}
```

---

### 2. Injeção de Dependência no `Program.cs`

```csharp
using Caching.Helpers.Extensions;

// Alternância transparente entre Redis e Modo Custo Zero ($0) conforme configuração
builder.Services.AddTlCaching(builder.Configuration);

// Ou registrar explicitamente o Modo Custo Zero em memória:
// builder.Services.AddMemoryCacheService();

// Ou registrar explicitamente o Redis com connection string direta:
// builder.Services.AddRedisCache(builder.Configuration);
```

---

### 3. Proteção contra Cache Stampede com Double-Checked Locking e Jitter

```csharp
using Caching.Helpers;
using Caching.Helpers.Resilience;

public class CatalogoService
{
    private readonly LockedCacheService _cacheService;

    public CatalogoService(LockedCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<ProdutoDto>> ObterCatalogoAsync()
    {
        // Aplica jitter de ±10% a ±15% sobre o tempo base de 30 minutos
        var ttlComJitter = TtlJitterCalculator.ApplyJitter(TimeSpan.FromMinutes(30));

        return await _cacheService.GetOrSetWithLockAsync(
            key: "catalogo:produtos",
            factory: async () => await ConsultarBancoDeDadosAsync(),
            expiration: ttlComJitter
        );
    }

    private static async Task<List<ProdutoDto>> ConsultarBancoDeDadosAsync()
    {
        await Task.Delay(100);
        return new List<ProdutoDto>
        {
            new(1, "Notebook Corporativo", 5200.00m),
            new(2, "Monitor UltraWide 34\"", 2400.00m)
        };
    }
}

public record ProdutoDto(int Id, string Nome, decimal Preco);
```

---

### 4. Criptografia e Proteção de Dados em Repouso

```csharp
using System.Security.Cryptography;
using Caching.Helpers.Compression;
using Caching.Helpers.Pipeline;
using Caching.Helpers.Security;

// Chave simétrica de 256 bits (32 bytes) carregada de cofre de chaves seguro (Key Vault / KMS)
var chaveMestra = Convert.FromBase64String("sua-chave-secreta-em-base64-de-32-bytes=");

var compression = new GZipCompressionProvider();
using var encryption = new AesEncryptionProvider(chaveMestra);

// Configura o pipeline ordenado: GZip -> Criptografia
var pipeline = new CachePayloadPipeline(compression, encryption);

// Serializa, comprime se > 1 KB e cifra com integridade de tag
var payloadCifrado = pipeline.Encode(sessaoUsuario);

// Decifra validando tag (rejeita adulteração), descomprime e deserializa
var sessaoRestaurada = pipeline.Decode<SessaoUsuarioDto>(payloadCifrado);
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre governança arquitetural, resiliência, FinOps e modelagem criptográfica:
- 📄 [ADR-001: Arquitetura do Ecossistema TL.DataHelpers e Segurança](../docs/ADR-001-arquitetura-ecossistema-datahelpers-e-seguranca.md)
- 📄 [ADR-000: Convenções e Padrões Globais](../docs/adr/ADR-000-arquitetura-e-convencoes.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).