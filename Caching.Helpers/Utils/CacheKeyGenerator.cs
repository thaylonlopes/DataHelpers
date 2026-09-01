namespace Caching.Helpers.Utils;

/// <summary>
/// Utilitário para padronização e composição de chaves de cache com suporte a regiões e tenants.
/// </summary>
public static class CacheKeyGenerator
{
    /// <summary>
    /// Gera uma chave de cache formatada, prefixando a região quando informada.
    /// </summary>
    /// <param name="key">Chave base do cache.</param>
    /// <param name="region">Região ou tenant opcional.</param>
    /// <returns>Chave composta formatada.</returns>
    public static string GenerateKey(string key, string? region = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return string.IsNullOrEmpty(region) ? key : $"{region}:{key}";
    }
}
