using AuditLogger.Interfaces;
using AuditLogger.Models;
using Serilog;

namespace AuditLogger.Services;

/// <summary>
/// Provedor de armazenamento e despacho de logs de auditoria via Serilog.
/// </summary>
public class SerilogAuditLogStorage : IAuditLogStorage
{
    private readonly ILogger _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="SerilogAuditLogStorage"/> com configuração padrão para console.
    /// </summary>
    public SerilogAuditLogStorage()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="SerilogAuditLogStorage"/> com uma instância customizada de <see cref="ILogger"/>.
    /// </summary>
    public SerilogAuditLogStorage(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void SaveLog(AuditLogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);
        _logger.Information("Audit Log: {Operation} by {UserId} on {Entity} at {Timestamp} with data {Data}",
            logEntry.Operation, logEntry.UserId, logEntry.Entity, logEntry.Timestamp, logEntry.Data);
    }
}
