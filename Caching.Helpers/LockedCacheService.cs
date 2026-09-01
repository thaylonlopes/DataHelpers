using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using RedLockNet;

namespace Caching.Helpers;

/// <summary>
/// Decorator para sincronização distribuída via <see cref="IDistributedLockFactory"/> prevenindo Cache Stampede.
/// </summary>
public class LockedCacheService : ICacheService
{
    private readonly ICacheService _innerCacheService;
    private readonly IDistributedLockFactory _lockFactory;
    private readonly TimeSpan _lockExpiry;
    private readonly TimeSpan _lockWait;
    private readonly TimeSpan _lockRetry;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="LockedCacheService"/>.
    /// </summary>
    public LockedCacheService(
        ICacheService innerCacheService,
        IDistributedLockFactory? lockFactory = null,
        TimeSpan? lockExpiry = null,
        TimeSpan? lockWait = null,
        TimeSpan? lockRetry = null)
    {
        _innerCacheService = innerCacheService ?? throw new ArgumentNullException(nameof(innerCacheService));
        _lockFactory = lockFactory ?? RedisLockFactory.GetFactory();
        _lockExpiry = lockExpiry ?? TimeSpan.FromSeconds(30);
        _lockWait = lockWait ?? TimeSpan.FromSeconds(10);
        _lockRetry = lockRetry ?? TimeSpan.FromSeconds(1);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, string? region = null)
    {
        return await _innerCacheService.GetAsync<T>(key, region);
    }

    /// <summary>
    /// Obtém um item do cache ou executa o factory protegido por lock distribuído contra Cache Stampede (Double-Checked Locking).
    /// </summary>
    /// <typeparam name="T">O tipo do valor.</typeparam>
    /// <param name="key">Chave de cache.</param>
    /// <param name="factory">Função assíncrona geradora de dados caso ocorra cache miss.</param>
    /// <param name="expiration">Tempo de expiração no cache.</param>
    /// <param name="slidingExpiration">Indica expiração deslizante.</param>
    /// <param name="region">Região opcional.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O valor recuperado do cache ou gerado pela fábrica.</returns>
    public async Task<T> GetOrSetWithLockAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration,
        bool slidingExpiration = false,
        string? region = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var cached = await _innerCacheService.GetAsync<T>(key, region);
        if (cached is not null)
        {
            return cached;
        }

        var resource = $"lock:stampede:{key}";
        using (var redLock = await _lockFactory.CreateLockAsync(resource, _lockExpiry, _lockWait, _lockRetry, ct))
        {
            if (redLock == null || !redLock.IsAcquired)
            {
                throw new InvalidOperationException($"Não foi possível adquirir o lock distribuído para o recurso: '{resource}'");
            }

            cached = await _innerCacheService.GetAsync<T>(key, region);
            if (cached is not null)
            {
                return cached;
            }

            var freshValue = await factory();
            if (freshValue is not null)
            {
                await _innerCacheService.SetAsync(key, freshValue, expiration, slidingExpiration, region);
            }

            return freshValue!;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
    {
        var resource = $"lock:{key}";
        using (var redLock = await _lockFactory.CreateLockAsync(resource, _lockExpiry, _lockWait, _lockRetry, default))
        {
            if (redLock != null && redLock.IsAcquired)
            {
                await _innerCacheService.SetAsync(key, value, expiration, slidingExpiration, region);
            }
            else
            {
                throw new InvalidOperationException($"Could not acquire lock for resource: '{resource}'");
            }
        }
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, string? region = null)
    {
        var resource = $"lock:{key}";
        using (var redLock = await _lockFactory.CreateLockAsync(resource, _lockExpiry, _lockWait, _lockRetry, default))
        {
            if (redLock != null && redLock.IsAcquired)
            {
                await _innerCacheService.RemoveAsync(key, region);
            }
            else
            {
                throw new InvalidOperationException($"Could not acquire lock for resource: '{resource}'");
            }
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateRegionAsync(string region)
    {
        var resource = $"lock:region:{region}";
        using (var redLock = await _lockFactory.CreateLockAsync(resource, _lockExpiry, _lockWait, _lockRetry, default))
        {
            if (redLock != null && redLock.IsAcquired)
            {
                await _innerCacheService.InvalidateRegionAsync(region);
            }
            else
            {
                throw new InvalidOperationException($"Could not acquire lock for resource: '{resource}'");
            }
        }
    }
}
