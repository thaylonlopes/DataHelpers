namespace Caching.Helpers.Interfaces;

/// <summary>
/// Contrato para serviços de pré-aquecimento (warm-up/priming) de cache para evitar sobrecarga inicial.
/// </summary>
public interface ICachePrimingService
{
    /// <summary>
    /// Pré-carrega um conjunto de dados no cache de forma assíncrona.
    /// </summary>
    /// <param name="dataToPreload">Dicionário contendo os pares de chave e valor a serem inseridos previamente no cache.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão do pré-aquecimento.</returns>
    Task PreloadCacheAsync(Dictionary<string, object> dataToPreload);
}
