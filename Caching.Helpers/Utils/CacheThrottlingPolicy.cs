using Polly;
using Polly.RateLimit;

namespace Caching.Helpers.Utils;

/// <summary>
/// Fábrica de políticas de limitação de taxa (throttling) para proteção contra sobrecarga em operações de cache.
/// </summary>
public static class CacheThrottlingPolicy
{
    /// <summary>
    /// Cria uma política assíncrona de limitação de taxa baseada em contagem máxima de execuções por intervalo temporal.
    /// </summary>
    /// <param name="numberOfExecutions">Número máximo de execuções permitidas no período.</param>
    /// <param name="perTimeSpan">Intervalo temporal correspondente ao teto de execuções.</param>
    /// <returns>A instância de <see cref="AsyncRateLimitPolicy"/> configurada.</returns>
    public static AsyncRateLimitPolicy CreateThrottlingPolicy(int numberOfExecutions, TimeSpan perTimeSpan)
    {
        return Policy.RateLimitAsync(numberOfExecutions, perTimeSpan);
    }
}
