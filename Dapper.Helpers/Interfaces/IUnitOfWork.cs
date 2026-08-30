using System.Data;

namespace Dapper.Helpers.Interfaces;

/// <summary>
/// Contrato para gestão de transações atômicas multi-repositório com Dapper.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// A conexão ativa de banco de dados.
    /// </summary>
    IDbConnection Connection { get; }

    /// <summary>
    /// A transação ativa de banco de dados, caso tenha sido iniciada.
    /// </summary>
    IDbTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Indica se existe uma transação ativa em andamento.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Inicia uma nova transação assíncrona de banco de dados.
    /// </summary>
    /// <param name="isolationLevel">Nível de isolamento da transação (padrão ReadCommitted).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>A transação iniciada.</returns>
    Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default);

    /// <summary>
    /// Confirma todas as operações executadas na transação ativa.
    /// </summary>
    /// <param name="ct">Token de cancelamento.</param>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Desfaz todas as operações executadas na transação ativa.
    /// </summary>
    /// <param name="ct">Token de cancelamento.</param>
    Task RollbackAsync(CancellationToken ct = default);

    /// <summary>
    /// Executa uma função dentro de uma transação atômica gerenciada automaticamente com commit ou rollback em caso de falha.
    /// </summary>
    /// <typeparam name="TResult">O tipo de retorno.</typeparam>
    /// <param name="action">A ação assíncrona a ser executada.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O resultado da operação.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<IDbTransaction, Task<TResult>> action, CancellationToken ct = default);
}

