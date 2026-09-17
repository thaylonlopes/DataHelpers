using Caching.Helpers;
using Caching.Helpers.Extensions;
using Caching.Helpers.Implementations;
using Caching.Helpers.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Caching.Helpers.Tests;

public class CacheServiceExtensionsTests
{
    [Fact]
    public void AddTlCaching_ShouldRegisterMemoryCacheService_WhenUseRedisIsFalse()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Cache:UseRedis", "false" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var services = new ServiceCollection();

        services.AddTlCaching(configuration);
        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        cacheService.Should().NotBeNull();
        cacheService.Should().BeOfType<MemoryCacheService>();
    }

    [Fact]
    public void AddTlCaching_ShouldRegisterMemoryCacheService_WhenRedisConnectionStringIsMissing()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Cache:UseRedis", "true" },
            { "Cache:RedisConnectionString", "" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var services = new ServiceCollection();

        services.AddTlCaching(configuration);
        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        cacheService.Should().NotBeNull();
        cacheService.Should().BeOfType<MemoryCacheService>();
    }

    [Fact]
    public void AddTlCaching_ShouldRegisterRedisCacheService_WhenUseRedisIsTrueAndConnectionStringProvided()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Cache:UseRedis", "true" },
            { "Cache:RedisConnectionString", "localhost:6379" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        var services = new ServiceCollection();

        services.AddTlCaching(configuration);
        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        cacheService.Should().NotBeNull();
        cacheService.Should().BeOfType<RedisCacheService>();
    }

    [Fact]
    public void AddMemoryCacheService_ShouldExplicitlyRegisterMemoryCacheService()
    {
        var services = new ServiceCollection();

        services.AddMemoryCacheService();
        var provider = services.BuildServiceProvider();
        var cacheService = provider.GetService<ICacheService>();

        cacheService.Should().NotBeNull();
        cacheService.Should().BeOfType<MemoryCacheService>();
    }
}
