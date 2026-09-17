using System.Text.Json;
using Caching.Helpers.Compression;
using Caching.Helpers.Interfaces;
using Caching.Helpers.Pipeline;
using Caching.Helpers.Utils;
using Microsoft.Extensions.Caching.Distributed;

namespace Caching.Helpers.Implementations;

/// <summary>
/// Provedor de cache distribuído baseado no Redis com suporte a pipeline ordenado de compressão e criptografia.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CachePayloadPipeline _pipeline;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="RedisCacheService"/>.
    /// </summary>
    /// <param name="cache">Instância de cache distribuído.</param>
    /// <param name="pipeline">Pipeline opcional de compressão e criptografia. Quando nulo, utiliza compressão GZip padrão.</param>
    public RedisCacheService(IDistributedCache cache, CachePayloadPipeline? pipeline = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _pipeline = pipeline ?? new CachePayloadPipeline(new GZipCompressionProvider());
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
        var data = await _cache.GetAsync(cacheKey);
        if (data is null)
        {
            return default;
        }

        return _pipeline.Decode<T>(data);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var options = CreateEntryOptions(expiration, slidingExpiration);
        var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
        var payload = _pipeline.Encode(value);

        await _cache.SetAsync(cacheKey, payload, options);

        if (!string.IsNullOrEmpty(region))
        {
            await AddKeyToRegionAsync(region, cacheKey);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
        await _cache.RemoveAsync(cacheKey);

        if (!string.IsNullOrEmpty(region))
        {
            await RemoveKeyFromRegionAsync(region, cacheKey);
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateRegionAsync(string region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        var keys = await GetKeysByRegionAsync(region);
        foreach (var key in keys)
        {
            await _cache.RemoveAsync(key);
        }
    }

    private static DistributedCacheEntryOptions CreateEntryOptions(TimeSpan expiration, bool slidingExpiration)
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
        return options;
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
