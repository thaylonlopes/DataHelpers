using Caching.Helpers.Interfaces;
using Caching.Helpers.Resilience;
using Caching.Helpers.Utils;
using Microsoft.Extensions.Logging;
using RedLockNet;

namespace Caching.Helpers;

/// <summary>
/// Decorator para sincronização distribuída via <see cref="IDistributedLockFactory"/> prevenindo Cache Stampede com Double-Checked Locking e Silent Fallback.
/// </summary>
public class LockedCacheService : ICacheService
{
    private readonly ICacheService _innerCacheService;
    private readonly IDistributedLockFactory _lockFactory;
    private readonly ILogger<LockedCacheService>? _logger;
    private readonly TimeSpan _lockExpiry;
    private readonly TimeSpan _lockWait;
    private readonly TimeSpan _lockRetry;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="LockedCacheService"/>.
    /// </summary>
    public LockedCacheService(
        ICacheService innerCacheService,
        IDistributedLockFactory? lockFactory = null,
        ILogger<LockedCacheService>? logger = null,
        TimeSpan? lockExpiry = null,
        TimeSpan? lockWait = null,
        TimeSpan? lockRetry = null)
    {
        _innerCacheService = innerCacheService ?? throw new ArgumentNullException(nameof(innerCacheService));
        _lockFactory = lockFactory ?? RedisLockFactory.GetFactory();
        _logger = logger;
        _lockExpiry = lockExpiry ?? TimeSpan.FromSeconds(30);
        _lockWait = lockWait ?? TimeSpan.FromSeconds(10);
        _lockRetry = lockRetry ?? TimeSpan.FromSeconds(1);
    }

    /// <inheritdoc/>
    public Task<T?> GetAsync<T>(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _innerCacheService.GetAsync<T>(key, region);
    }

    /// <summary>
    /// Obtém um item do cache ou executa a factory sob Double-Checked Locking com Distributed Lock e Silent Fallback.
    /// Leituras com cache hit são resolvidas sem aquisição de lock.
    /// </summary>
    /// <typeparam name="T">O tipo do valor.</typeparam>
    /// <param name="key">Chave de cache.</param>
    /// <param name="factory">Função assíncrona geradora de dados em caso de cache miss.</param>
    /// <param name="expiration">Tempo de expiração no cache.</param>
    /// <param name="slidingExpiration">Indica expiração deslizante.</param>
    /// <param name="region">Região opcional.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O valor recuperado do cache ou gerado pela fábrica.</returns>
    public Task<T> GetOrSetWithLockAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration,
        bool slidingExpiration = false,
        string? region = null,
        CancellationToken ct = default)
    {
        return CacheLockHelper.ExecuteWithDoubleCheckedLockAsync(
            _innerCacheService,
            _lockFactory,
            key,
            factory,
            expiration,
            slidingExpiration,
            region,
            _logger,
            _lockExpiry,
            _lockWait,
            _lockRetry,
            ct);
    }

    /// <inheritdoc/>
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _innerCacheService.SetAsync(key, value, expiration, slidingExpiration, region);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _innerCacheService.RemoveAsync(key, region);
    }

    /// <inheritdoc/>
    public Task InvalidateRegionAsync(string region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(region);
        return _innerCacheService.InvalidateRegionAsync(region);
    }
}
