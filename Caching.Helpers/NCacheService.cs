using Alachisoft.NCache.Client;
using Alachisoft.NCache.Runtime.Caching;
using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using System.Text.Json;

namespace Caching.Helpers
{
    public class NCacheService : ICacheService
    {
        private readonly ICache _cache;

        public NCacheService(string cacheName)
        {
            _cache = CacheManager.GetCache(cacheName);
        }

        public Task<T?> GetAsync<T>(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var value = _cache.Get<byte[]>(cacheKey);
            if (value is null) return Task.FromResult<T?>(default);
            var jsonData = CompressionHelper.Decompress(value);
            return Task.FromResult(JsonSerializer.Deserialize<T>(jsonData));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var jsonData = JsonSerializer.Serialize(value);
            var compressedData = CompressionHelper.Compress(jsonData);
            var cacheItem = new CacheItem(compressedData)
            {
                Expiration = new Expiration(ExpirationType.Absolute, expiration)
            };
            _cache.Insert(cacheKey, cacheItem);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }

        public Task InvalidateRegionAsync(string region)
        {
            _cache.Clear();
            return Task.CompletedTask;
        }
    }
}
