using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Caching.Helpers.Utils;
using DataHelpers.Benchmarks.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DataHelpers.Benchmarks.Suites;

[MemoryDiagnoser]
public class CachingBenchmarks
{
    private string _largeJsonPayload = null!;
    private byte[] _compressedPayload = null!;
    private MemoryCache _memoryCache = null!;
    private readonly string _cacheKey = "tenant_sp:orders_catalog_2026";

    [GlobalSetup]
    public void Setup()
    {
        var orders = new List<OrderDto>();
        for (int i = 0; i < 50; i++)
        {
            orders.Add(new OrderDto
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-2026-{i:D4}",
                CustomerName = $"Cliente Corporativo {i}",
                CustomerEmail = $"cliente{i}@empresa.com.br",
                TotalAmount = 1500.00m + (i * 25.5m),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ItemCount = i + 1,
                City = "Sao Paulo",
                Country = "Brasil"
            });
        }
        _largeJsonPayload = JsonSerializer.Serialize(orders);
        _compressedPayload = CompressionHelper.Compress(_largeJsonPayload);

        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _memoryCache.Set(_cacheKey, _compressedPayload, TimeSpan.FromMinutes(30));
    }

    [Benchmark(Baseline = true, Description = "1. Payload: Raw JSON Storage")]
    public byte[] Cache_Payload_RawUtf8()
    {
        return Encoding.UTF8.GetBytes(_largeJsonPayload);
    }

    [Benchmark(Description = "1. Payload: CompressionHelper GZip")]
    public byte[] Cache_Payload_GZip()
    {
        return CompressionHelper.Compress(_largeJsonPayload);
    }

    [Benchmark(Description = "2. KeyGen: String Interpolation Naive")]
    public string Cache_KeyGen_Interpolation()
    {
        var tenant = "tenant_sp";
        var entity = "orders_catalog";
        var year = 2026;
        return $"{tenant}:{entity}_{year}";
    }

    [Benchmark(Description = "2. KeyGen: CacheKeyGenerator Deterministic")]
    public string Cache_KeyGen_Generator()
    {
        return CacheKeyGenerator.GenerateKey("orders_catalog_2026", "tenant_sp");
    }

    [Benchmark(Description = "3. Cache Read: Simulated Serialized Store")]
    public byte[] Cache_Read_SimulatedL2()
    {
        var bytes = Encoding.UTF8.GetBytes(_largeJsonPayload);
        return JsonSerializer.Deserialize<byte[]>(JsonSerializer.Serialize(bytes)) ?? bytes;
    }

    [Benchmark(Description = "3. Cache Read: L1 MemoryCache Fast Hit")]
    public byte[]? Cache_Read_L1MemoryHit()
    {
        return _memoryCache.Get<byte[]>(_cacheKey);
    }
}
