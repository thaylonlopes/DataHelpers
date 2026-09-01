using Apache.Ignite.Core;
using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using System.Text.Json;

namespace Caching.Helpers
{
    public class IgniteCacheService : ICacheService
    {
        private readonly IIgnite _ignite;

        public IgniteCacheService(IIgnite ignite)
        {
            _ignite = ignite;
        }

        public Task<T?> GetAsync<T>(string key, string? region = null)
        {
            var cache = _ignite.GetOrCreateCache<string, byte[]>(region ?? "default");
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var value = cache.Get(cacheKey);
            if (value is null) return Task.FromResult<T?>(default);
            var jsonData = CompressionHelper.Decompress(value);
            return Task.FromResult(JsonSerializer.Deserialize<T>(jsonData));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
        {
            var cache = _ignite.GetOrCreateCache<string, byte[]>(region ?? "default");
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var jsonData = JsonSerializer.Serialize(value);
            var compressedData = CompressionHelper.Compress(jsonData);
            cache.Put(cacheKey, compressedData);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, string? region = null)
        {
            var cache = _ignite.GetOrCreateCache<string, object>(region ?? "default");
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            cache.Remove(cacheKey);
            return Task.CompletedTask;
        }

        public Task InvalidateRegionAsync(string region)
        {
            _ignite.DestroyCache(region ?? "default");
            return Task.CompletedTask;
        }
    }
}
