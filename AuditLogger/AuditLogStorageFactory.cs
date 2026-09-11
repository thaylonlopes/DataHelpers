using AuditLogger.Interfaces;
using AuditLogger.Models;
using AuditLogger.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AuditLogger;

/// <summary>
/// Fábrica para criação de instâncias de armazenamento de logs de auditoria (<see cref="IAuditLogStorage"/>).
/// </summary>
public static class AuditLogStorageFactory
{
    /// <summary>
    /// Cria o provedor de armazenamento correspondente às configurações informadas.
    /// </summary>
    /// <param name="options">Opções de configuração do AuditLogger.</param>
    /// <param name="logger">Instância opcional de logger.</param>
    /// <returns>Instância de <see cref="IAuditLogStorage"/>.</returns>
    public static IAuditLogStorage Create(AuditLoggerSettings options, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.StorageType switch
        {
            "Logger" or "ILogger" or "Console" or "Serilog" or "NLog" => logger != null
                ? new LoggerAuditLogStorage(logger)
                : new LoggerAuditLogStorage(),
            _ => new MemoryCacheAuditLogStorage(new MemoryCache(new MemoryCacheOptions()), options.CacheDuration)
        };
    }
}