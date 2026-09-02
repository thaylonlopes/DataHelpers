using System.Reflection;
using System.Text.Json;
using AuditLogger.Interfaces;
using AuditLogger.Models;

namespace AuditLogger;

/// <summary>
/// Implementação de referência para registro de auditoria com serialização JSON estruturada, cálculo automático de diffs e despacho para storage configurado.
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly IAuditLogStorage _logStorage;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="AuditLogger"/>.
    /// </summary>
    /// <param name="logStorage">Provedor de armazenamento de logs.</param>
    public AuditLogger(IAuditLogStorage logStorage)
    {
        _logStorage = logStorage ?? throw new ArgumentNullException(nameof(logStorage));
    }

    /// <inheritdoc/>
    public Guid LogCreate<T>(T item, string userId)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = new AuditLogEntry("CREATE", userId, typeof(T).Name, JsonSerializer.Serialize(item));
        _logStorage.SaveLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public Guid LogRead<T>(T item, string userId)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = new AuditLogEntry("READ", userId, typeof(T).Name, JsonSerializer.Serialize(item));
        _logStorage.SaveLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public Guid LogUpdate<T>(T oldItem, T newItem, string userId)
    {
        ArgumentNullException.ThrowIfNull(oldItem);
        ArgumentNullException.ThrowIfNull(newItem);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var structuredData = CreateStructuredUpdatePayload(oldItem, newItem);
        var logEntry = new AuditLogEntry("UPDATE", userId, typeof(T).Name, structuredData);
        _logStorage.SaveLog(logEntry);
        return logEntry.Id;
    }

    /// <inheritdoc/>
    public Guid LogDelete<T>(T item, string userId)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var logEntry = new AuditLogEntry("DELETE", userId, typeof(T).Name, JsonSerializer.Serialize(item));
        _logStorage.SaveLog(logEntry);
        return logEntry.Id;
    }

    private static string CreateStructuredUpdatePayload<T>(T oldItem, T newItem)
    {
        var diffMap = new Dictionary<string, object?>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in properties)
        {
            var oldVal = prop.GetValue(oldItem);
            var newVal = prop.GetValue(newItem);

            if (!Equals(oldVal, newVal))
            {
                diffMap[prop.Name] = new
                {
                    From = oldVal,
                    To = newVal
                };
            }
        }

        var payload = new
        {
            Old = oldItem,
            New = newItem,
            Diff = diffMap
        };

        return JsonSerializer.Serialize(payload);
    }
}
