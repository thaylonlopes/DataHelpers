using DataMapping.Helpers;
using DataMapping.Helpers.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro do SimpleMapper no contêiner de DI.
/// </summary>
public static class DataMappingExtensions
{
    /// <summary>
    /// Registra o <see cref="SimpleMapper"/> como singleton implementando <see cref="IMapper"/>.
    /// </summary>
    /// <param name="services">Coleção de serviços de injeção de dependência.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddSimpleMapper(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IMapper, SimpleMapper>();
        return services;
    }
}

