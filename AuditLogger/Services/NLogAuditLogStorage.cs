using AuditLogger.Interfaces;
using AuditLogger.Models;
using NLog;

namespace AuditLogger.Services;

/// <summary>
/// Provedor de armazenamento e despacho de logs de auditoria via NLog.
/// </summary>
public class NLogAuditLogStorage : IAuditLogStorage
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    /// <inheritdoc/>
    public void SaveLog(AuditLogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);
        _logger.Info($"Audit Log: {logEntry.Operation} by {logEntry.UserId} on {logEntry.Entity} at {logEntry.Timestamp} with data {logEntry.Data}");
    }
}