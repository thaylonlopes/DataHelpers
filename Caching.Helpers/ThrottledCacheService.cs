using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;
using Polly.RateLimit;

namespace Caching.Helpers;

/// <summary>
/// Decorator para controle de vazão (throttling) e resiliência utilizando políticas de Rate Limiting da Polly.
/// </summary>
public class ThrottledCacheService : ICacheService
{
    private readonly ICacheService _innerCacheService;
    private readonly AsyncRateLimitPolicy _throttlingPolicy;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="ThrottledCacheService"/>.
    /// </summary>
    /// <param name="innerCacheService">Serviço de cache subjacente.</param>
    /// <param name="numberOfExecutions">Número máximo de execuções permitidas por intervalo.</param>
    /// <param name="perTimeSpan">Intervalo de tempo para controle de taxa.</param>
    public ThrottledCacheService(ICacheService innerCacheService, int numberOfExecutions, TimeSpan perTimeSpan)
    {
        _innerCacheService = innerCacheService ?? throw new ArgumentNullException(nameof(innerCacheService));
        _throttlingPolicy = CacheThrottlingPolicy.CreateThrottlingPolicy(numberOfExecutions, perTimeSpan);
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, string? region = null)
    {
        return await _throttlingPolicy.ExecuteAsync(() => _innerCacheService.GetAsync<T>(key, region));
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null)
    {
        await _throttlingPolicy.ExecuteAsync(() => _innerCacheService.SetAsync(key, value, expiration, slidingExpiration, region));
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, string? region = null)
    {
        await _throttlingPolicy.ExecuteAsync(() => _innerCacheService.RemoveAsync(key, region));
    }

    /// <inheritdoc/>
    public async Task InvalidateRegionAsync(string region)
    {
        await _throttlingPolicy.ExecuteAsync(() => _innerCacheService.InvalidateRegionAsync(region));
    }
}
