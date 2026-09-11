using AuditLogger.Models;

namespace AuditLogger.Interfaces;

/// <summary>
/// Contrato para provedores de persistência de entradas de log de auditoria.
/// </summary>
public interface IAuditLogStorage
{
    /// <summary>
    /// Salva uma entrada de auditoria no meio de persistência configurado.
    /// </summary>
    /// <param name="logEntry">A entrada de auditoria a ser armazenada.</param>
    void SaveLog(AuditLogEntry logEntry);

    /// <summary>
    /// Salva assincronamente uma entrada de auditoria no meio de persistência configurado.
    /// </summary>
    /// <param name="logEntry">A entrada de auditoria a ser armazenada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task SaveLogAsync(AuditLogEntry logEntry, CancellationToken cancellationToken = default)
    {
        SaveLog(logEntry);
        return Task.CompletedTask;
    }
}
