using Caching.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Caching.Helpers.Tests;

public class MemoryCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheService _service;

    public MemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new MemoryCacheService(_memoryCache);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_WhenKeyDoesNotExist()
    {
        var result = await _service.GetAsync<string>("chave-inexistente");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_And_GetAsync_ShouldStoreAndRetrieveValue()
    {
        const string key = "cliente:123";
        const string value = "Thaylon Lopes";

        await _service.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var result = await _service.GetAsync<string>(key);

        result.Should().Be(value);
    }

    [Fact]
    public async Task RemoveAsync_ShouldEvictValueFromCache()
    {
        const string key = "chave:remover";
        await _service.SetAsync(key, "valor-temporario", TimeSpan.FromMinutes(5));

        await _service.RemoveAsync(key);
        var result = await _service.GetAsync<string>(key);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateRegionAsync_ShouldEvictAllKeysInSpecifiedRegion()
    {
        const string region = "tenant-42";
        await _service.SetAsync("item1", "valor1", TimeSpan.FromMinutes(10), region: region);
        await _service.SetAsync("item2", "valor2", TimeSpan.FromMinutes(10), region: region);
        await _service.SetAsync("outro-item", "valor3", TimeSpan.FromMinutes(10), region: "tenant-99");

        await _service.InvalidateRegionAsync(region);

        var item1 = await _service.GetAsync<string>("item1", region);
        var item2 = await _service.GetAsync<string>("item2", region);
        var outro = await _service.GetAsync<string>("outro-item", "tenant-99");

        item1.Should().BeNull();
        item2.Should().BeNull();
        outro.Should().Be("valor3");
    }

    [Fact]
    public async Task SetAsync_ShouldRespectExpiration()
    {
        const string key = "chave:curta";
        await _service.SetAsync(key, "dado", TimeSpan.FromMilliseconds(50));

        await Task.Delay(100);
        var result = await _service.GetAsync<string>(key);

        result.Should().BeNull();
    }
}
