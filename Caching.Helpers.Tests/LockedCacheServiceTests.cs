using Caching.Helpers.Interfaces;
using FluentAssertions;
using Moq;
using RedLockNet;
using Xunit;

namespace Caching.Helpers.Tests;

public class LockedCacheServiceTests
{
    private readonly Mock<ICacheService> _innerCacheMock;
    private readonly Mock<IDistributedLockFactory> _lockFactoryMock;
    private readonly Mock<IRedLock> _redLockMock;
    private readonly LockedCacheService _lockedCacheService;

    public LockedCacheServiceTests()
    {
        _innerCacheMock = new Mock<ICacheService>();
        _lockFactoryMock = new Mock<IDistributedLockFactory>();
        _redLockMock = new Mock<IRedLock>();

        _lockFactoryMock.Setup(f => f.CreateLockAsync(
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_redLockMock.Object);

        _lockFactoryMock.Setup(f => f.CreateLockAsync(
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken?>()))
            .ReturnsAsync(_redLockMock.Object);

        _lockedCacheService = new LockedCacheService(_innerCacheMock.Object, _lockFactoryMock.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldDelegateDirectlyToInnerCache_WithoutLock()
    {
        _innerCacheMock.Setup(c => c.GetAsync<string>("key:1", null))
            .ReturnsAsync("cached_val");

        var result = await _lockedCacheService.GetAsync<string>("key:1");

        result.Should().Be("cached_val");
    }

    [Fact]
    public async Task SetAsync_ShouldAcquireLockAndCallInnerCache_WhenLockAcquired()
    {
        _redLockMock.Setup(r => r.IsAcquired).Returns(true);

        await _lockedCacheService.SetAsync("key:2", "value:2", TimeSpan.FromMinutes(5));

        _innerCacheMock.Verify(c => c.SetAsync("key:2", "value:2", TimeSpan.FromMinutes(5), false, null), Times.Once);
        _redLockMock.Verify(r => r.Dispose(), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldThrowInvalidOperationException_WhenLockNotAcquired()
    {
        _redLockMock.Setup(r => r.IsAcquired).Returns(false);

        var act = () => _lockedCacheService.SetAsync("key:3", "value:3", TimeSpan.FromMinutes(5));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Could not acquire lock*");
        _innerCacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_ShouldAcquireLockAndCallInnerCache_WhenLockAcquired()
    {
        _redLockMock.Setup(r => r.IsAcquired).Returns(true);

        await _lockedCacheService.RemoveAsync("key:4");

        _innerCacheMock.Verify(c => c.RemoveAsync("key:4", null), Times.Once);
    }

    [Fact]
    public async Task InvalidateRegionAsync_ShouldAcquireLockForRegion()
    {
        _redLockMock.Setup(r => r.IsAcquired).Returns(true);

        await _lockedCacheService.InvalidateRegionAsync("tenant-99");

        _innerCacheMock.Verify(c => c.InvalidateRegionAsync("tenant-99"), Times.Once);
    }

    [Fact]
    public async Task GetOrSetWithLockAsync_ShouldReturnCachedValue_WithoutLock_WhenAlreadyInCache()
    {
        _innerCacheMock.Setup(c => c.GetAsync<string>("stampede:1", null))
            .ReturnsAsync("cached_result");

        var factoryCalled = false;

        var result = await _lockedCacheService.GetOrSetWithLockAsync("stampede:1", () =>
        {
            factoryCalled = true;
            return Task.FromResult("fresh");
        }, TimeSpan.FromMinutes(10));

        result.Should().Be("cached_result");
        factoryCalled.Should().BeFalse();
        _lockFactoryMock.Verify(f => f.CreateLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetOrSetWithLockAsync_ShouldAcquireLockAndExecuteFactory_OnCacheMiss()
    {
        _innerCacheMock.Setup(c => c.GetAsync<string>("stampede:2", null))
            .ReturnsAsync((string?)null);
        _redLockMock.Setup(r => r.IsAcquired).Returns(true);

        var factoryCalled = false;

        var result = await _lockedCacheService.GetOrSetWithLockAsync("stampede:2", () =>
        {
            factoryCalled = true;
            return Task.FromResult("fresh_generated");
        }, TimeSpan.FromMinutes(10));

        result.Should().Be("fresh_generated");
        factoryCalled.Should().BeTrue();
        _innerCacheMock.Verify(c => c.SetAsync("stampede:2", "fresh_generated", TimeSpan.FromMinutes(10), false, null), Times.Once);
    }
}
