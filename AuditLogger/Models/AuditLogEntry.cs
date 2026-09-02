namespace AuditLogger.Models;

/// <summary>
/// Representa uma entrada imutável no log de auditoria.
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
    /// Cria uma nova entrada de log de auditoria com timestamp UTC atual.
    /// </summary>
    public AuditLogEntry(string operation, string userId, string entity, string data)
    {
        Id = Guid.NewGuid();
        Operation = operation;
        UserId = userId;
        Entity = entity;
        Data = data;
        Timestamp = DateTime.UtcNow;
    }
}
