using Caching.Helpers.Interfaces;
using Microsoft.Extensions.Logging;
using RedLockNet;

namespace Caching.Helpers.Resilience;

/// <summary>
/// Utilitário para execução de consultas protegidas por Double-Checked Locking com Distributed Lock e Silent Fallback.
/// </summary>
public static class CacheLockHelper
{
    private static readonly TimeSpan DefaultLockExpiry = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultLockWait = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultLockRetry = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Executa o padrão Double-Checked Locking contra Cache Stampede, com fallback silencioso para a factory caso o Redis falhe.
    /// </summary>
    public static async Task<T> ExecuteWithDoubleCheckedLockAsync<T>(
        ICacheService cache,
        IDistributedLockFactory lockFactory,
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration,
        bool slidingExpiration = false,
        string? region = null,
        ILogger? logger = null,
        TimeSpan? lockExpiry = null,
        TimeSpan? lockWait = null,
        TimeSpan? lockRetry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(lockFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var initialCached = await TryGetFromCacheAsync<T>(cache, key, region, logger);
        if (initialCached is not null)
        {
            return initialCached;
        }

        var expiry = lockExpiry ?? DefaultLockExpiry;
        var wait = lockWait ?? DefaultLockWait;
        var retry = lockRetry ?? DefaultLockRetry;
        var resourceKey = BuildLockResourceKey(key);

        IRedLock? redLock = null;
        try
        {
            redLock = await AcquireDistributedLockSafelyAsync(lockFactory, resourceKey, expiry, wait, retry, cancellationToken, logger);
            if (redLock is null || !redLock.IsAcquired)
            {
                logger?.LogWarning("Lock distribuído não pôde ser adquirido para '{Key}'. Ativando fallback silencioso.", key);
                return await factory();
            }

            var verifiedCached = await TryGetFromCacheAsync<T>(cache, key, region, logger);
            if (verifiedCached is not null)
            {
                return verifiedCached;
            }

            var freshValue = await factory();
            if (freshValue is not null)
            {
                await TrySetToCacheAsync(cache, key, freshValue, expiration, slidingExpiration, region, logger);
            }

            return freshValue!;
        }
        finally
        {
            redLock?.Dispose();
        }
    }

    private static string BuildLockResourceKey(string key)
    {
        return $"lock:stampede:{key}";
    }

    private static async Task<T?> TryGetFromCacheAsync<T>(ICacheService cache, string key, string? region, ILogger? logger)
    {
        try
        {
            return await cache.GetAsync<T>(key, region);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Falha transitória ao ler chave '{Key}' do cache.", key);
            return default;
        }
    }

    private static async Task TrySetToCacheAsync<T>(
        ICacheService cache,
        string key,
        T value,
        TimeSpan expiration,
        bool slidingExpiration,
        string? region,
        ILogger? logger)
    {
        try
        {
            await cache.SetAsync(key, value, expiration, slidingExpiration, region);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Falha transitória ao gravar chave '{Key}' no cache após recálculo.", key);
        }
    }

    private static async Task<IRedLock?> AcquireDistributedLockSafelyAsync(
        IDistributedLockFactory lockFactory,
        string resourceKey,
        TimeSpan expiry,
        TimeSpan wait,
        TimeSpan retry,
        CancellationToken cancellationToken,
        ILogger? logger)
    {
        try
        {
            return await lockFactory.CreateLockAsync(resourceKey, expiry, wait, retry, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Erro de conectividade ao solicitar lock distribuído para recurso '{Resource}'.", resourceKey);
            return null;
        }
    }
}
