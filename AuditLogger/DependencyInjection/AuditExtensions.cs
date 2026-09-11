using AuditLogger.Interfaces;
using AuditLogger.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuditLogger.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro de serviços de auditoria no contêiner de DI.
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Registra os serviços do AuditLogger no <see cref="IServiceCollection"/> a partir de uma configuração.
    /// </summary>
    public static IServiceCollection AddAuditLogger(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var settings = new AuditLoggerSettings();
        var section = configuration.GetSection("AuditLoggerSettings");
        if (section.Exists())
        {
            if (TimeSpan.TryParse(section["CacheDuration"], out var cacheDuration))
            {
                settings.CacheDuration = cacheDuration;
            }

            if (!string.IsNullOrWhiteSpace(section["StorageType"]))
            {
                settings.StorageType = section["StorageType"]!;
            }

            if (bool.TryParse(section["EnableHeuristicMasking"], out var enableHeuristic))
            {
                settings.EnableHeuristicMasking = enableHeuristic;
            }

            if (!string.IsNullOrWhiteSpace(section["DefaultMask"]))
            {
                settings.DefaultMask = section["DefaultMask"]!;
            }
        }

        return AddAuditLogger(services, opt =>
        {
            opt.CacheDuration = settings.CacheDuration;
            opt.StorageType = settings.StorageType;
            opt.EnableHeuristicMasking = settings.EnableHeuristicMasking;
            opt.DefaultMask = settings.DefaultMask;
        });
    }

    /// <summary>
    /// Registra os serviços do AuditLogger no <see cref="IServiceCollection"/> com opções programáticas opcionais.
    /// </summary>
    public static IServiceCollection AddAuditLogger(this IServiceCollection services, Action<AuditLoggerSettings>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.AddOptions<AuditLoggerSettings>();
        }

        return RegisterAuditServices(services);
    }

    private static IServiceCollection RegisterAuditServices(IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddSingleton<IAuditLogStorage>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<AuditLoggerSettings>>().Value;
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("AuditLogger");
            return AuditLogStorageFactory.Create(options, logger);
        });

        services.AddSingleton<IAuditLogger>(provider =>
        {
            var storage = provider.GetRequiredService<IAuditLogStorage>();
            var logger = provider.GetService<ILogger<AuditLogger>>();
            var options = provider.GetService<IOptions<AuditLoggerSettings>>();
            return new AuditLogger(storage, logger, options);
        });

        services.AddSingleton(provider => (AuditLogger)provider.GetRequiredService<IAuditLogger>());

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
