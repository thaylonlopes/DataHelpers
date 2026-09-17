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

        _lockedCacheService = new LockedCacheService(_innerCacheMock.Object, _lockFactoryMock.Object);
    }

    [Fact]
    public async Task GetAsync_ShouldDelegateDirectlyToInnerCache_WithoutLock()
    {
        _innerCacheMock.Setup(c => c.GetAsync<string>("key:1", null))
            .ReturnsAsync("cached_val");

        var result = await _lockedCacheService.GetAsync<string>("key:1");

        result.Should().Be("cached_val");
        _lockFactoryMock.Verify(f => f.CreateLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetAsync_ShouldOperateLockFree_CallingInnerCacheDirectly()
    {
        await _lockedCacheService.SetAsync("key:2", "value:2", TimeSpan.FromMinutes(5));

        _innerCacheMock.Verify(c => c.SetAsync("key:2", "value:2", TimeSpan.FromMinutes(5), false, null), Times.Once);
        _lockFactoryMock.Verify(f => f.CreateLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_ShouldOperateLockFree_CallingInnerCacheDirectly()
    {
        await _lockedCacheService.RemoveAsync("key:4");

        _innerCacheMock.Verify(c => c.RemoveAsync("key:4", null), Times.Once);
        _lockFactoryMock.Verify(f => f.CreateLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InvalidateRegionAsync_ShouldOperateLockFree_CallingInnerCacheDirectly()
    {
        await _lockedCacheService.InvalidateRegionAsync("tenant-99");

        _innerCacheMock.Verify(c => c.InvalidateRegionAsync("tenant-99"), Times.Once);
        _lockFactoryMock.Verify(f => f.CreateLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
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
        _redLockMock.Verify(r => r.Dispose(), Times.Once);
    }

    [Fact]
    public async Task GetOrSetWithLockAsync_ShouldFallbackSilentlyToFactory_WhenLockAcquisitionFails()
    {
        _innerCacheMock.Setup(c => c.GetAsync<string>("stampede:fallback", null))
            .ReturnsAsync((string?)null);
        _redLockMock.Setup(r => r.IsAcquired).Returns(false);

        var result = await _lockedCacheService.GetOrSetWithLockAsync("stampede:fallback", () =>
        {
            return Task.FromResult("resultado_banco_direto");
        }, TimeSpan.FromMinutes(10));

        result.Should().Be("resultado_banco_direto");
    }

    [Fact]
    public async Task GetOrSetWithLockAsync_ShouldFallbackSilentlyToFactory_WhenDistributedLockThrowsException()
    {
        _innerCacheMock.Setup(c => c.GetAsync<string>("stampede:exception", null))
            .ReturnsAsync((string?)null);
        _lockFactoryMock.Setup(f => f.CreateLockAsync(
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Redis cluster connection timeout."));

        var result = await _lockedCacheService.GetOrSetWithLockAsync("stampede:exception", () =>
        {
            return Task.FromResult("dado_recuperado_com_sucesso");
        }, TimeSpan.FromMinutes(10));

        result.Should().Be("dado_recuperado_com_sucesso");
    }

    [Fact]
    public async Task GetOrSetWithLockAsync_ShouldExecuteFactoryOnlyOnce_WhenMultipleConcurrentRequestsOccur()
    {
        string? sharedCacheStore = null;
        var factoryExecutionCount = 0;

        _innerCacheMock.Setup(c => c.GetAsync<string>("concorrencia:key", null))
            .Returns(() => Task.FromResult(sharedCacheStore));

        _innerCacheMock.Setup(c => c.SetAsync(
            "concorrencia:key",
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<bool>(),
            It.IsAny<string?>()))
            .Callback<string, string, TimeSpan, bool, string?>((_, val, _, _, _) => sharedCacheStore = val)
            .Returns(Task.CompletedTask);

        var simulatedLock = new SemaphoreSlim(1, 1);
        _lockFactoryMock.Setup(f => f.CreateLockAsync(
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await simulatedLock.WaitAsync();
                var redLockMock = new Mock<IRedLock>();
                redLockMock.Setup(r => r.IsAcquired).Returns(true);
                redLockMock.Setup(r => r.Dispose()).Callback(() => simulatedLock.Release());
                return redLockMock.Object;
            });

        var tasks = Enumerable.Range(0, 50).Select(_ => _lockedCacheService.GetOrSetWithLockAsync(
            "concorrencia:key",
            async () =>
            {
                Interlocked.Increment(ref factoryExecutionCount);
                await Task.Delay(20);
                return "resultado_unico_compartilhado";
            },
            TimeSpan.FromMinutes(5)
        ));

        var results = await Task.WhenAll(tasks);

        results.Should().AllBeEquivalentTo("resultado_unico_compartilhado");
        factoryExecutionCount.Should().Be(1);
    }
}
