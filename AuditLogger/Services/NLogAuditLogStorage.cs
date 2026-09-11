using AuditLogger.Interfaces;
using AuditLogger.Models;
using Microsoft.Extensions.Logging;

namespace AuditLogger.Services;

/// <summary>
/// Provedor de compatibilidade que repassa o registro de auditoria para <see cref="ILogger"/>.
/// </summary>
public class NLogAuditLogStorage : LoggerAuditLogStorage
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="NLogAuditLogStorage"/>.
    /// </summary>
    public NLogAuditLogStorage()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="NLogAuditLogStorage"/> com a abstração <see cref="ILogger"/>.
    /// </summary>
    public NLogAuditLogStorage(ILogger logger) : base(logger)
    {
    }
}