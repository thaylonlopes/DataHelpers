namespace AuditLogger.Interfaces;

/// <summary>
/// Contrato para registro de trilhas de auditoria para operações CRUD.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Registra uma operação de criação de entidade.
    /// </summary>
    Guid LogCreate<T>(T item, string userId);

    /// <summary>
    /// Registra uma operação de leitura de entidade.
    /// </summary>
    Guid LogRead<T>(T item, string userId);

    /// <summary>
    /// Registra uma operação de atualização comparando o estado anterior e o novo estado da entidade.
    /// </summary>
    Guid LogUpdate<T>(T oldItem, T newItem, string userId);

    /// <summary>
    /// Registra uma operação de exclusão de entidade.
    /// </summary>
    Guid LogDelete<T>(T item, string userId);

    /// <summary>
    /// Registra assincronamente uma operação de criação de entidade.
    /// </summary>
    Task<Guid> LogCreateAsync<T>(T item, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra assincronamente uma operação de leitura de entidade.
    /// </summary>
    Task<Guid> LogReadAsync<T>(T item, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra assincronamente uma operação de atualização comparando o estado anterior e o novo estado da entidade.
    /// </summary>
    Task<Guid> LogUpdateAsync<T>(T oldItem, T newItem, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra assincronamente uma operação de exclusão de entidade.
    /// </summary>
    Task<Guid> LogDeleteAsync<T>(T item, string userId, CancellationToken cancellationToken = default);
}
