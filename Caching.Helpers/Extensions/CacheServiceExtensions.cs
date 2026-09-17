using Caching.Helpers.Configurations;
using Caching.Helpers.Implementations;
using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Caching.Helpers.Extensions;

/// <summary>
/// Métodos de extensão para registro de serviços de caching no contêiner de injeção de dependência.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Registra o provedor de cache padrão da suíte TL.DataHelpers com alternância transparente entre Redis e Modo Custo Zero ($0).
    /// </summary>
    /// <param name="services">Coleção de serviços do contêiner.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <returns>A coleção de serviços configurada.</returns>
    public static IServiceCollection AddTlCaching(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var useRedis = ResolveUseRedis(configuration);
        var connectionString = ResolveRedisConnectionString(configuration);

        if (useRedis && !string.IsNullOrWhiteSpace(connectionString))
        {
            RegisterRedisProvider(services, connectionString);
        }
        else
        {
            RegisterMemoryProvider(services);
        }

        return services;
    }

    /// <summary>
    /// Registra explicitamente o provedor distribuído Redis.
    /// </summary>
    /// <param name="services">Coleção de serviços do contêiner.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <returns>A coleção de serviços configurada.</returns>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = ResolveRedisConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        RegisterRedisProvider(services, connectionString);
        return services;
    }

    /// <summary>
    /// Registra explicitamente o provedor em memória local Modo Custo Zero ($0).
    /// </summary>
    /// <param name="services">Coleção de serviços do contêiner.</param>
    /// <returns>A coleção de serviços configurada.</returns>
    public static IServiceCollection AddMemoryCacheService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        RegisterMemoryProvider(services);
        return services;
    }

    /// <summary>
    /// Pré-carrega dados no cache durante a inicialização da aplicação.
    /// </summary>
    public static async Task<IServiceCollection> AddCacheWithPrimingAsync(this IServiceCollection services, Dictionary<string, object> dataToPreload)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(dataToPreload);

        services.AddSingleton<ICachePrimingService, CachePrimingService>();
        var serviceProvider = services.BuildServiceProvider();
        var cachePrimingService = serviceProvider.GetRequiredService<ICachePrimingService>();

        await cachePrimingService.PreloadCacheAsync(dataToPreload);
        return services;
    }

    /// <summary>
    /// Registra decorador de controle de vazão (throttling) para chamadas ao cache.
    /// </summary>
    public static IServiceCollection AddThrottledCache(this IServiceCollection services, int numberOfExecutions, TimeSpan perTimeSpan)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICacheService>(sp =>
        {
            var innerCacheService = new RedisCacheService(sp.GetRequiredService<IDistributedCache>());
            return new ThrottledCacheService(innerCacheService, numberOfExecutions, perTimeSpan);
        });
        return services;
    }

    /// <summary>
    /// Registra decorador de locking distribuído anti-stampede.
    /// </summary>
    public static IServiceCollection AddDistributedLockingCache<T>(this IServiceCollection services, string connectionString) where T : class, ICacheService
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        RedisLockFactory.Initialize(connectionString);

        services.AddSingleton<ICacheService>(sp =>
        {
            var innerCacheService = sp.GetRequiredService<T>();
            return new LockedCacheService(innerCacheService);
        });
        return services;
    }

    /// <summary>
    /// Registra provedor baseado em SQLite para persistência em arquivo local.
    /// </summary>
    public static IServiceCollection AddSQLiteCache(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ICacheService, SQLiteCacheService>();
        return services;
    }

    private static bool ResolveUseRedis(IConfiguration configuration)
    {
        return configuration.GetValue<bool?>("Cache:UseRedis")
            ?? configuration.GetValue<bool?>("CacheSettings:UseRedis")
            ?? false;
    }

    private static string? ResolveRedisConnectionString(IConfiguration configuration)
    {
        return configuration.GetValue<string>("Cache:RedisConnectionString")
            ?? configuration.GetValue<string>("CacheSettings:RedisConnectionString");
    }

    private static void RegisterRedisProvider(IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
    }

    private static void RegisterMemoryProvider(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
    }
}
