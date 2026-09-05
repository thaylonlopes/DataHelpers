using Caching.Helpers.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Caching.Helpers.Tests;

public class HierarchicalCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ICacheService> _distributedCacheMock;
    private readonly HierarchicalCacheService _hierarchicalCache;

    public HierarchicalCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _distributedCacheMock = new Mock<ICacheService>();
        _hierarchicalCache = new HierarchicalCacheService(_memoryCache, _distributedCacheMock.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnFromMemoryL1Directly_WhenKeyExistsInL1()
    {
        var key = "item:1";
        var cachedValue = new TestData { Id = 1, Name = "Item L1" };
        _memoryCache.Set(key, cachedValue, TimeSpan.FromMinutes(10));

        var result = await _hierarchicalCache.GetAsync<TestData>(key);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Item L1");
        _distributedCacheMock.Verify(d => d.GetAsync<TestData>(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_ShouldFetchFromL2AndPopulateL1_WhenL1MissAndL2Hit()
    {
        var key = "item:2";
        var distributedValue = new TestData { Id = 2, Name = "Item L2" };
        _distributedCacheMock.Setup(d => d.GetAsync<TestData>(key, null))
            .ReturnsAsync(distributedValue);

        var firstResult = await _hierarchicalCache.GetAsync<TestData>(key);

        firstResult.Should().NotBeNull();
        firstResult!.Name.Should().Be("Item L2");
        _distributedCacheMock.Verify(d => d.GetAsync<TestData>(key, null), Times.Once);

        var secondResult = await _hierarchicalCache.GetAsync<TestData>(key);
        secondResult.Should().NotBeNull();
        secondResult!.Name.Should().Be("Item L2");
        _distributedCacheMock.Verify(d => d.GetAsync<TestData>(key, null), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldSaveInBothL1AndL2()
    {
        var key = "item:3";
        var data = new TestData { Id = 3, Name = "Dual Write" };
        var expiration = TimeSpan.FromMinutes(15);

        await _hierarchicalCache.SetAsync(key, data, expiration);

        _memoryCache.TryGetValue(key, out TestData? memVal).Should().BeTrue();
        memVal!.Name.Should().Be("Dual Write");
        _distributedCacheMock.Verify(d => d.SetAsync(key, data, expiration, false, null), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldEvictFromBothL1AndL2()
    {
        var key = "item:4";
        _memoryCache.Set(key, new TestData { Id = 4, Name = "To Remove" });

        await _hierarchicalCache.RemoveAsync(key);

        _memoryCache.TryGetValue(key, out _).Should().BeFalse();
        _distributedCacheMock.Verify(d => d.RemoveAsync(key, null), Times.Once);
    }

    [Fact]
    public async Task InvalidateRegionAsync_ShouldDelegateToDistributedCache()
    {
        var region = "tenant-1";

        await _hierarchicalCache.InvalidateRegionAsync(region);

        _distributedCacheMock.Verify(d => d.InvalidateRegionAsync(region), Times.Once);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

