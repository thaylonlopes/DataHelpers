using Apache.Ignite.Core;
using Caching.Helpers.Configurations;
using Caching.Helpers.Implementations;
using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Caching.Helpers.Extensions
{
    public static class CacheServiceExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>()
                ?? throw new InvalidOperationException("CacheSettings configuration section is missing or invalid.");

            if (string.IsNullOrWhiteSpace(cacheSettings.RedisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheSettings.RedisConnectionString;
            });

            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }

        public static IServiceCollection AddMemcachedCache(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>()
                ?? throw new InvalidOperationException("CacheSettings configuration section is missing or invalid.");

            if (string.IsNullOrWhiteSpace(cacheSettings.MemcachedServer))
            {
                throw new InvalidOperationException("Memcached server is not configured.");
            }

            services.AddEnyimMemcached(options =>
            {
                options.AddServer(cacheSettings.MemcachedServer, 11211);
            });

            services.AddSingleton<ICacheService, MemcachedCacheService>();
            return services;
        }
        public static async Task<IServiceCollection> AddCacheWithPrimingAsync(this IServiceCollection services, Dictionary<string, object> dataToPreload)
        {
            services.AddSingleton<ICachePrimingService, CachePrimingService>();
            var serviceProvider = services.BuildServiceProvider();
            var cachePrimingService = serviceProvider.GetRequiredService<ICachePrimingService>();

            await cachePrimingService.PreloadCacheAsync(dataToPreload);
            return services;
        }
        public static IServiceCollection AddThrottledCache(this IServiceCollection services, int numberOfExecutions, TimeSpan perTimeSpan)
        {
            services.AddSingleton<ICacheService>(sp =>
            {
                var innerCacheService = new RedisCacheService(sp.GetRequiredService<IDistributedCache>());
                return new ThrottledCacheService(innerCacheService, numberOfExecutions, perTimeSpan);
            });
            return services;
        }
        public static IServiceCollection AddDistributedLockingCache<T>(this IServiceCollection services, string connectionString) where T : class, ICacheService
        {
            RedisLockFactory.Initialize(connectionString);

            services.AddSingleton<ICacheService>(sp =>
            {
                var innerCacheService = sp.GetRequiredService<T>();
                return new LockedCacheService(innerCacheService);
            });
            return services;

        }
        public static IServiceCollection AddSQLiteCache(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService, SQLiteCacheService>();
            return services;
        }
        public static IServiceCollection AddNCache(this IServiceCollection services, string cacheName)
        {
            services.AddSingleton<ICacheService>(sp =>
            {
                return new NCacheService(cacheName);
            });
            return services;
        }
        public static IServiceCollection AddIgniteCache(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService>(sp =>
            {
                var ignite = Ignition.Start();
                return new IgniteCacheService(ignite);
            });
            return services;
        }

    }
}
