using Microsoft.Extensions.Caching.Memory;
using PagingFiltering.Helpers.Interfaces;
using PagingFiltering.Helpers.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro dos serviços de paginação e filtragem no contêiner de DI.
/// </summary>
public static class PagingFilteringExtensions
{
    /// <summary>
    /// Registra os serviços de paginação, filtragem e cache em memória no contêiner de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços de injeção de dependência.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddPagingFiltering(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}

