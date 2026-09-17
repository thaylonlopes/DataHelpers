namespace Caching.Helpers.Resilience;

/// <summary>
/// Calculador de dispersão pseudo-aleatória de expiração (TTL Jitter) para prevenção de Cache Stampede.
/// </summary>
public static class TtlJitterCalculator
{
    private const double DefaultMinJitterRatio = 0.10;
    private const double DefaultMaxJitterRatio = 0.15;

    /// <summary>
    /// Aplica uma dispersão de ±10% a ±15% ao tempo de vida (TTL) base informado.
    /// </summary>
    /// <param name="baseTtl">Tempo de expiração base.</param>
    /// <returns>Tempo de expiração com jitter aplicado.</returns>
    public static TimeSpan ApplyJitter(TimeSpan baseTtl)
    {
        return ApplyJitter(baseTtl, DefaultMinJitterRatio, DefaultMaxJitterRatio);
    }

    /// <summary>
    /// Aplica uma dispersão percentual customizada ao tempo de vida (TTL) base informado.
    /// </summary>
    /// <param name="baseTtl">Tempo de expiração base.</param>
    /// <param name="minJitterRatio">Percentual mínimo de jitter (ex: 0.10 para 10%).</param>
    /// <param name="maxJitterRatio">Percentual máximo de jitter (ex: 0.15 para 15%).</param>
    /// <returns>Tempo de expiração com jitter aplicado.</returns>
    public static TimeSpan ApplyJitter(TimeSpan baseTtl, double minJitterRatio, double maxJitterRatio)
    {
        ValidateJitterParameters(baseTtl, minJitterRatio, maxJitterRatio);

        var randomFactor = GenerateRandomFactor(minJitterRatio, maxJitterRatio);
        var sign = Random.Shared.Next(2) == 0 ? 1.0 : -1.0;
        var deltaMilliseconds = sign * randomFactor * baseTtl.TotalMilliseconds;
        var jitteredMilliseconds = baseTtl.TotalMilliseconds + deltaMilliseconds;

        return EnsurePositiveTimeSpan(jitteredMilliseconds);
    }

    private static void ValidateJitterParameters(TimeSpan baseTtl, double minJitterRatio, double maxJitterRatio)
    {
        if (baseTtl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(baseTtl), "O TTL base deve ser maior que zero.");
        }

        if (minJitterRatio < 0 || maxJitterRatio < minJitterRatio)
        {
            throw new ArgumentException("Intervalo de jitter inválido. O percentual mínimo deve ser maior ou igual a zero e menor ou igual ao máximo.");
        }
    }

    private static double GenerateRandomFactor(double minRatio, double maxRatio)
    {
        return minRatio + (Random.Shared.NextDouble() * (maxRatio - minRatio));
    }

    private static TimeSpan EnsurePositiveTimeSpan(double milliseconds)
    {
        var minimumMilliseconds = 1000.0;
        var effectiveMilliseconds = Math.Max(milliseconds, minimumMilliseconds);
        return TimeSpan.FromMilliseconds(effectiveMilliseconds);
    }
}
