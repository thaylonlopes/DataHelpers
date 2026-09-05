using Caching.Helpers.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Caching.Helpers.Tests;

public class ThrottledCacheServiceTests
{
    private readonly Mock<ICacheService> _innerCacheMock;
    private readonly ThrottledCacheService _throttledCacheService;

    public ThrottledCacheServiceTests()
    {
        _innerCacheMock = new Mock<ICacheService>();
        _throttledCacheService = new ThrottledCacheService(_innerCacheMock.Object, 100, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetAsync_ShouldDelegateToInnerCacheUnderLimit()
    {
        _innerCacheMock.Setup(c => c.GetAsync<string>("key:1", null))
            .ReturnsAsync("val:1");

        var result = await _throttledCacheService.GetAsync<string>("key:1");

        result.Should().Be("val:1");
        _innerCacheMock.Verify(c => c.GetAsync<string>("key:1", null), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldDelegateToInnerCacheUnderLimit()
    {
        await _throttledCacheService.SetAsync("key:2", "val:2", TimeSpan.FromMinutes(1));

        _innerCacheMock.Verify(c => c.SetAsync("key:2", "val:2", TimeSpan.FromMinutes(1), false, null), Times.Once);
    }
}

