using System.Collections.Concurrent;
using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace Caching.Helpers;

/// <summary>
/// Provedor de cache em memória local de alta performance implementando o Modo Custo Zero ($0).
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _regionIndex = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Inicializa uma nova instância de <see cref="MemoryCacheService"/>.
    /// </summary>
    /// <param name="memoryCache">Instância de <see cref="IMemoryCache"/> a ser utilizada.</param>
    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
        if (_memoryCache.TryGetValue(cacheKey, out T? value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult(default(T));
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
        var entryOptions = CreateEntryOptions(expiration, slidingExpiration);

        _memoryCache.Set(cacheKey, value, entryOptions);

        if (!string.IsNullOrEmpty(region))
        {
            TrackRegionKey(region, cacheKey);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var cacheKey = CacheKeyGenerator.GenerateKey(key, region);
        _memoryCache.Remove(cacheKey);

        if (!string.IsNullOrEmpty(region))
        {
            UntrackRegionKey(region, cacheKey);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task InvalidateRegionAsync(string region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        if (_regionIndex.TryRemove(region, out var keys))
        {
            foreach (var key in keys.Keys)
            {
                _memoryCache.Remove(key);
            }
        }

        return Task.CompletedTask;
    }

    private static MemoryCacheEntryOptions CreateEntryOptions(TimeSpan expiration, bool slidingExpiration)
    {
        var options = new MemoryCacheEntryOptions();
        if (slidingExpiration)
        {
            options.SetSlidingExpiration(expiration);
        }
        else
        {
            options.SetAbsoluteExpiration(expiration);
        }
        return options;
    }

    private void TrackRegionKey(string region, string cacheKey)
    {
        var keys = _regionIndex.GetOrAdd(region, _ => new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase));
        keys.TryAdd(cacheKey, 0);
    }

    private void UntrackRegionKey(string region, string cacheKey)
    {
        if (_regionIndex.TryGetValue(region, out var keys))
        {
            keys.TryRemove(cacheKey, out _);
        }
    }
}
