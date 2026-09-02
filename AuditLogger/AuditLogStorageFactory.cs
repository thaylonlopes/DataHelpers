using AuditLogger.Interfaces;
using AuditLogger.Models;
using AuditLogger.Services;
using Microsoft.Extensions.Caching.Memory;

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
    /// <returns>Instância de <see cref="IAuditLogStorage"/>.</returns>
    public static IAuditLogStorage Create(AuditLoggerSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.StorageType switch
        {
            "Serilog" => new SerilogAuditLogStorage(),
            "NLog" => new NLogAuditLogStorage(),
            _ => new MemoryCacheAuditLogStorage(new MemoryCache(new MemoryCacheOptions()), options.CacheDuration)
        };
    }
}