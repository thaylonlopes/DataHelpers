namespace Caching.Helpers.Configurations;

/// <summary>
/// Configurações para serviços de caching distribuído e em memória.
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// String de conexão para o servidor ou cluster Redis.
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Indica se o cache distribuído Redis deve ser utilizado.
    /// Quando definido como falso ou se a conexão não for informada, ativa o Modo Custo Zero ($0) em memória.
    /// </summary>
    public bool UseRedis { get; set; }
}
