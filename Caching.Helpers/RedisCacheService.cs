using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Caching.Helpers.Implementations
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetAsync<T>(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var data = await _cache.GetAsync(cacheKey);
            if (data is null) return default;
            var jsonData = CompressionHelper.Decompress(data);
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
        {
            var options = new DistributedCacheEntryOptions();
            if (slidingExpiration)
            {
                options.SlidingExpiration = expiration;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var jsonData = JsonSerializer.Serialize(value);
            var compressedData = CompressionHelper.Compress(jsonData);
            await _cache.SetAsync(cacheKey, compressedData, options);
            if (!string.IsNullOrEmpty(region))
            {
                await AddKeyToRegionAsync(region, cacheKey);
            }
        }

        public async Task RemoveAsync(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            await _cache.RemoveAsync(cacheKey);
            if (!string.IsNullOrEmpty(region))
            {
                await RemoveKeyFromRegionAsync(region, cacheKey);
            }
        }

        public async Task InvalidateRegionAsync(string region)
        {
            var keys = await GetKeysByRegionAsync(region);
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key);
            }
        }

        private async Task AddKeyToRegionAsync(string region, string cacheKey)
        {
            var regionKey = $"region:{region}";
            var existingKeys = await _cache.GetStringAsync(regionKey);
            var keys = existingKeys == null
                ? new HashSet<string>()
                : JsonSerializer.Deserialize<HashSet<string>>(existingKeys) ?? new HashSet<string>();

            keys.Add(cacheKey);
            var updatedKeys = JsonSerializer.Serialize(keys);
            await _cache.SetStringAsync(regionKey, updatedKeys);
        }

        private async Task RemoveKeyFromRegionAsync(string region, string cacheKey)
        {
            var regionKey = $"region:{region}";
            var existingKeys = await _cache.GetStringAsync(regionKey);
            if (existingKeys != null)
            {
                var keys = JsonSerializer.Deserialize<HashSet<string>>(existingKeys) ?? new HashSet<string>();
                if (keys.Remove(cacheKey))
                {
                    var updatedKeys = JsonSerializer.Serialize(keys);
                    await _cache.SetStringAsync(regionKey, updatedKeys);
                }
            }
        }

        private async Task<IEnumerable<string>> GetKeysByRegionAsync(string region)
        {
            var regionKey = $"region:{region}";
            var existingKeys = await _cache.GetStringAsync(regionKey);
            return existingKeys == null
                ? Enumerable.Empty<string>()
                : JsonSerializer.Deserialize<IEnumerable<string>>(existingKeys) ?? Enumerable.Empty<string>();
        }
    }
}
