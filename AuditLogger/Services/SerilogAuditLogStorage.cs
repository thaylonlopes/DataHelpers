using AuditLogger.Interfaces;
using AuditLogger.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuditLogger.Services;

/// <summary>
/// Provedor de armazenamento e despacho de logs de auditoria baseado na abstração <see cref="ILogger"/>.
/// </summary>
public class LoggerAuditLogStorage : IAuditLogStorage
{
    private readonly ILogger _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="LoggerAuditLogStorage"/> com um logger nulo padrão.
    /// </summary>
    public LoggerAuditLogStorage()
        : this(NullLogger<LoggerAuditLogStorage>.Instance)
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="LoggerAuditLogStorage"/> com uma instância fornecida de <see cref="ILogger"/>.
    /// </summary>
    /// <param name="logger">Instância de logger.</param>
    public LoggerAuditLogStorage(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void SaveLog(AuditLogEntry logEntry)
    {
        ArgumentNullException.ThrowIfNull(logEntry);

        _logger.LogInformation(
            "Audit Log: {Operation} by {UserId} on {Entity} at {Timestamp} with correlation {CorrelationId} and tenant {TenantId} with data {Data}",
            logEntry.Operation,
            logEntry.UserId,
            logEntry.Entity,
            logEntry.Timestamp,
            logEntry.CorrelationId,
            logEntry.TenantId,
            logEntry.Data);
    }
}

/// <summary>
/// Provedor de compatibilidade que repassa o registro de auditoria para <see cref="ILogger"/>.
/// </summary>
public class SerilogAuditLogStorage : LoggerAuditLogStorage
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="SerilogAuditLogStorage"/>.
    /// </summary>
    public SerilogAuditLogStorage()
    {
    }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="SerilogAuditLogStorage"/> com a abstração <see cref="ILogger"/>.
    /// </summary>
    public SerilogAuditLogStorage(ILogger logger) : base(logger)
    {
    }
}
