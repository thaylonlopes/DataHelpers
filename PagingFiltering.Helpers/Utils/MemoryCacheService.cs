using Microsoft.Extensions.Caching.Memory;
using PagingFiltering.Helpers.Interfaces;

namespace PagingFiltering.Helpers.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string cacheKey)
        {
            _memoryCache.TryGetValue(cacheKey, out T? item);
            return Task.FromResult(item);
        }

        public Task SetAsync<T>(string cacheKey, T item, TimeSpan expiration)
        {
            _memoryCache.Set(cacheKey, item, expiration);
            return Task.CompletedTask;
        }

        public Task SetCachedDataAsync<T>(string cacheKey, T data)
        {
            _memoryCache.Set(cacheKey, data);
            return Task.CompletedTask;
        }
        public async Task SetCachedDataWithExpirationAsync<T>(string cacheKey, T data, TimeSpan expiration)
        {
            _memoryCache.Set(cacheKey, data, expiration);
            await Task.CompletedTask;
        }

    }
}
