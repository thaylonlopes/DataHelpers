using PagingFiltering.Helpers;
using PagingFiltering.Helpers.Implementations;
using PagingFiltering.Helpers.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro dos serviços de paginação e filtragem no contêiner de DI.
/// </summary>
public static class PagingFilteringExtensions
{
    /// <summary>
    /// Registra os serviços de paginação e filtragem no contêiner de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços de injeção de dependência.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddPagingFiltering(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPaginationHelper<>), typeof(PaginationHelper<>));
        services.AddTransient(typeof(IPaginationFilterHelper<>), typeof(PaginationFilterHelper<>));

        return services;
    }
}

