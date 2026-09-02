namespace AuditLogger.Models;

/// <summary>
/// Configurações de tempo de retenção e provedor de armazenamento do AuditLogger.
/// </summary>
public class AuditLoggerSettings
{
    /// <summary>
    /// Tempo de expiração do cache em memória quando utilizado MemoryCacheAuditLogStorage.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Tipo de armazenamento a ser utilizado (ex: "MemoryCache", "Serilog", "NLog").
    /// </summary>
    public string StorageType { get; set; } = "MemoryCache";
}
