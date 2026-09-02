using AuditLogger.Interfaces;
using AuditLogger.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuditLogger.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro de serviços de auditoria no contêiner de DI.
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Registra os serviços do AuditLogger no <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddAuditLogger(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AuditLoggerSettings>(configuration.GetSection("AuditLoggerSettings"));

        services.AddMemoryCache();
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<AuditLoggerSettings>>().Value;
            return AuditLogStorageFactory.Create(options);
        });

        services.AddSingleton<IAuditLogger, AuditLogger>();
        services.AddSingleton<AuditLogger>();

        return services;
    }

    /// <summary>
    /// Método legado para compatibilidade.
    /// </summary>
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddAuditLogger(services, configuration);
    }
}
