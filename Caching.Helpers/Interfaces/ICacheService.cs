namespace Caching.Helpers.Interfaces;

/// <summary>
/// Contrato unificado para serviços de caching distribuído e em memória com suporte a particionamento regional.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtém um item do cache de forma assíncrona com base na chave e região opcional.
    /// </summary>
    /// <typeparam name="T">O tipo do objeto armazenado no cache.</typeparam>
    /// <param name="key">A chave identificadora do item no cache.</param>
    /// <param name="region">Região opcional para isolamento lógico de chaves.</param>
    /// <returns>A instância armazenada ou o valor padrão caso a chave não exista ou esteja expirada.</returns>
    Task<T?> GetAsync<T>(string key, string? region = null);

    /// <summary>
    /// Armazena um item no cache de forma assíncrona com tempo de vida definido.
    /// </summary>
    /// <typeparam name="T">O tipo do objeto a ser armazenado.</typeparam>
    /// <param name="key">A chave identificadora do item.</param>
    /// <param name="value">A instância do objeto a ser armazenado.</param>
    /// <param name="expiration">Tempo de vida da chave antes da expiração.</param>
    /// <param name="slidingExpiration">Indica se a expiração é deslizante (renovada a cada acesso).</param>
    /// <param name="region">Região opcional para isolamento lógico.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da operação.</returns>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, bool slidingExpiration = false, string? region = null);

    /// <summary>
    /// Remove um item específico do cache de forma assíncrona.
    /// </summary>
    /// <param name="key">A chave identificadora do item a ser removido.</param>
    /// <param name="region">Região opcional onde a chave reside.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da remoção.</returns>
    Task RemoveAsync(string key, string? region = null);

    /// <summary>
    /// Invalida todas as chaves associadas a uma região específica de cache de forma assíncrona.
    /// </summary>
    /// <param name="region">O nome da região a ser invalidada.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da invalidação.</returns>
    Task InvalidateRegionAsync(string region);
}
