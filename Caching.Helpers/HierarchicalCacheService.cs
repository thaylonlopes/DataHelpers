using System.Diagnostics;
using Caching.Helpers.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace Caching.Helpers;

/// <summary>
/// Provedor de cache multinível combinando L1 (Memória Local) e L2 (Cache Distribuído) com métricas Prometheus.
/// </summary>
public class HierarchicalCacheService : IHierarchicalCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _distributedCache;
    private readonly ILogger<HierarchicalCacheService>? _logger;

    private static readonly Counter CacheHits = Metrics.CreateCounter("cache_hits_total", "Total cache hits.");
    private static readonly Counter CacheMisses = Metrics.CreateCounter("cache_misses_total", "Total cache misses.");

    /// <summary>
    /// Inicializa uma nova instância de <see cref="HierarchicalCacheService"/>.
    /// </summary>
    public HierarchicalCacheService(IMemoryCache memoryCache, ICacheService distributedCache, ILogger<HierarchicalCacheService>? logger = null)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var stopwatch = Stopwatch.StartNew();

        if (_memoryCache.TryGetValue(key, out T? value) && value is not null)
        {
            CacheHits.Inc();
            _logger?.LogInformation("Cache hit for key: {Key}", key);
            return value;
        }

        CacheMisses.Inc();
        var valueFromDistributed = await _distributedCache.GetAsync<T>(key, region);
        value = valueFromDistributed;

        stopwatch.Stop();
        _logger?.LogInformation("Cache miss for key: {Key}. Time taken: {Elapsed}ms", key, stopwatch.ElapsedMilliseconds);

        if (value != null)
        {
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
        }

        return value;
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _memoryCache.Set(key, value, expiration);
        await _distributedCache.SetAsync(key, value, expiration, slidingExpiration, region);
        _logger?.LogInformation("Cached key: {Key} with expiration: {Expiration}", key, expiration);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key, region);
        _logger?.LogInformation("Removed key: {Key} from cache", key);
    }

    /// <inheritdoc/>
    public async Task InvalidateRegionAsync(string region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        await _distributedCache.InvalidateRegionAsync(region);
        _logger?.LogInformation("Invalidated region: {Region}", region);
    }
}
