using System.Text.Json;
using Caching.Helpers.Interfaces;
using Caching.Helpers.Utils;

namespace Caching.Helpers;

/// <summary>
/// Serviço para pré-carregamento e aquecimento de cache em lote com compressão de dados.
/// </summary>
public class CachePrimingService : ICachePrimingService
{
    private readonly ICacheService _cacheService;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="CachePrimingService"/>.
    /// </summary>
    public CachePrimingService(ICacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <inheritdoc/>
    public async Task PreloadCacheAsync(Dictionary<string, object> dataToPreload)
    {
        ArgumentNullException.ThrowIfNull(dataToPreload);

        foreach (var item in dataToPreload)
        {
            var jsonData = JsonSerializer.Serialize(item.Value);
            var compressedData = CompressionHelper.Compress(jsonData);
            await _cacheService.SetAsync(item.Key, compressedData, TimeSpan.FromHours(1));
        }
    }
}
