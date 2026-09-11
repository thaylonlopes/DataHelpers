using System.Diagnostics;

namespace AuditLogger.Models;

/// <summary>
/// Provedor e gerenciador de contexto distribuído para trilhas de auditoria (CorrelationId e TenantId).
/// Permite correlação automática com traces W3C / OpenTelemetry.
/// </summary>
public static class AuditCorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();
    private static readonly AsyncLocal<string?> _tenantId = new();

    /// <summary>
    /// Identificador de correlação distribuída atual. Caso não definido explicitamente, extrai de <see cref="Activity.Current"/>.
    /// </summary>
    public static string? CorrelationId
    {
        get => _correlationId.Value ?? Activity.Current?.TraceId.ToString() ?? Activity.Current?.Id;
        set => _correlationId.Value = value;
    }

    /// <summary>
    /// Identificador do locatário (tenant) da requisição ou mensagem em processamento.
    /// </summary>
    public static string? TenantId
    {
        get => _tenantId.Value;
        set => _tenantId.Value = value;
    }

    /// <summary>
    /// Define o contexto de auditoria e retorna um escopo descartável para limpeza automática ao término.
    /// </summary>
    /// <param name="correlationId">Identificador de correlação.</param>
    /// <param name="tenantId">Identificador do locatário.</param>
    /// <returns>Objeto descartável que restaura o contexto anterior.</returns>
    public static IDisposable SetContext(string? correlationId, string? tenantId)
    {
        var previousCorrelation = _correlationId.Value;
        var previousTenant = _tenantId.Value;

        _correlationId.Value = correlationId;
        _tenantId.Value = tenantId;

        return new ContextScope(previousCorrelation, previousTenant);
    }

    private sealed class ContextScope : IDisposable
    {
        private readonly string? _previousCorrelation;
        private readonly string? _previousTenant;

        public ContextScope(string? previousCorrelation, string? previousTenant)
        {
            _previousCorrelation = previousCorrelation;
            _previousTenant = previousTenant;
        }

        public void Dispose()
        {
            _correlationId.Value = _previousCorrelation;
            _tenantId.Value = _previousTenant;
        }
    }
}

/// <summary>
/// Utilitário interno para higienização de quebras de linha em entradas de log.
/// </summary>
internal static class CrlfSanitizer
{
    public static string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return input.Replace("\r", "\\r").Replace("\n", "\\n");
    }

    public static string? SanitizeNullable(string? input)
    {
        if (input == null)
        {
            return null;
        }

        return Sanitize(input);
    }
}

/// <summary>
/// Representa uma entrada imutável no log de auditoria com metadados de contexto distribuído.
/// </summary>
public class AuditLogEntry
{
    /// <summary>
    /// Identificador único da entrada de auditoria.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Tipo de operação realizada (ex: CREATE, READ, UPDATE, DELETE).
    /// </summary>
    public string Operation { get; private set; }

    /// <summary>
    /// Identificador do usuário que executou a operação.
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// Nome da entidade ou tabela afetada.
    /// </summary>
    public string Entity { get; private set; }

    /// <summary>
    /// Dados serializados em JSON associados à operação.
    /// </summary>
    public string Data { get; private set; }

    /// <summary>
    /// Data e hora em UTC da ocorrência.
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Identificador de correlação distribuída (OpenTelemetry / W3C TraceId).
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Identificador do locatário (Tenant) no contexto multi-tenant.
    /// </summary>
    public string? TenantId { get; private set; }

    /// <summary>
    /// Cria uma nova entrada de log de auditoria com timestamp UTC atual e captura de contexto distribuído.
    /// </summary>
    public AuditLogEntry(
        string operation,
        string userId,
        string entity,
        string data,
        string? correlationId = null,
        string? tenantId = null)
    {
        Id = Guid.NewGuid();
        Operation = CrlfSanitizer.Sanitize(operation);
        UserId = CrlfSanitizer.Sanitize(userId);
        Entity = CrlfSanitizer.Sanitize(entity);
        Data = CrlfSanitizer.Sanitize(data);
        Timestamp = DateTime.UtcNow;
        CorrelationId = CrlfSanitizer.SanitizeNullable(correlationId ?? AuditCorrelationContext.CorrelationId);
        TenantId = CrlfSanitizer.SanitizeNullable(tenantId ?? AuditCorrelationContext.TenantId);
    }
}
