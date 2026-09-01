using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using Enyim.Caching;
using System.Text.Json;

namespace Caching.Helpers.Implementations
{
    public class MemcachedCacheService : ICacheService
    {
        private readonly IMemcachedClient _client;

        public MemcachedCacheService(IMemcachedClient client)
        {
            _client = client;
        }

        public async Task<T?> GetAsync<T>(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var value = await _client.GetAsync<byte[]>(cacheKey);
            if (!value.HasValue) return default;
            var jsonData = CompressionHelper.Decompress(value.Value);
            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            var jsonData = JsonSerializer.Serialize(value);
            var compressedData = CompressionHelper.Compress(jsonData);
            await _client.SetAsync(cacheKey, compressedData, (int)expiration.TotalSeconds);
            if (!string.IsNullOrEmpty(region))
            {
                await AddKeyToRegionAsync(region, cacheKey);
            }
        }

        public async Task RemoveAsync(string key, string? region = null)
        {
            var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
            await _client.RemoveAsync(cacheKey);
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
                await _client.RemoveAsync(key);
            }
        }

        private async Task AddKeyToRegionAsync(string region, string cacheKey)
        {
            var regionKey = $"region:{region}";
            var existingKeys = await _client.GetAsync<string>(regionKey);
            var keys = existingKeys == null
                ? new HashSet<string>()
                : JsonSerializer.Deserialize<HashSet<string>>((Stream)existingKeys) ?? new HashSet<string>();

            keys.Add(cacheKey);
            var updatedKeys = JsonSerializer.Serialize(keys);
            await _client.SetAsync(regionKey, updatedKeys, int.MaxValue);
        }

        private async Task RemoveKeyFromRegionAsync(string region, string cacheKey)
        {
            var regionKey = $"region:{region}";
            var existingKeys = await _client.GetAsync<string>(regionKey);
            if (existingKeys != null)
            {
                var keys = JsonSerializer.Deserialize<HashSet<string>>((Stream)existingKeys) ?? new HashSet<string>();
                if (keys.Remove(cacheKey))
                {
                    var updatedKeys = JsonSerializer.Serialize(keys);
                    await _client.SetAsync(regionKey, updatedKeys, int.MaxValue);
                }
            }
        }

        private async Task<IEnumerable<string>> GetKeysByRegionAsync(string region)
        {
            var regionKey = $"region:{region}";
            var existingKeys = await _client.GetAsync<string>(regionKey);
            return existingKeys == null
                ? Enumerable.Empty<string>()
                : JsonSerializer.Deserialize<IEnumerable<string>>((Stream)existingKeys) ?? Enumerable.Empty<string>();
        }
    }
}
