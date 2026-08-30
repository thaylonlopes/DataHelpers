using System.Data;
using Dapper.Helpers.Interfaces;

namespace Dapper.Helpers.Context;

/// <summary>
/// Implementação de Unit of Work transacional para Dapper com suporte a rollback automático e execução segura.
/// </summary>
public class DapperUnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DapperUnitOfWork"/>.
    /// </summary>
    /// <param name="connection">A conexão com o banco de dados.</param>
    public DapperUnitOfWork(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <inheritdoc />
    public IDbConnection Connection => _connection;

    /// <inheritdoc />
    public IDbTransaction? CurrentTransaction => _transaction;

    /// <inheritdoc />
    public bool HasActiveTransaction => _transaction != null;

    /// <inheritdoc />
    public Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken ct = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DapperUnitOfWork));
        if (_transaction != null) throw new InvalidOperationException("Já existe uma transação ativa nesta Unit of Work.");

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        _transaction = _connection.BeginTransaction(isolationLevel);
        return Task.FromResult(_transaction);
    }

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken ct = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DapperUnitOfWork));
        if (_transaction == null) throw new InvalidOperationException("Nenhuma transação ativa para efetuar commit.");

        try
        {
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken ct = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DapperUnitOfWork));
        if (_transaction == null) return Task.CompletedTask;

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<IDbTransaction, Task<TResult>> action, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var transaction = await BeginTransactionAsync(ct: ct);
        try
        {
            var result = await action(transaction);
            await CommitAsync(ct);
            return result;
        }
        catch
        {
            await RollbackAsync(ct);
            throw;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _transaction?.Dispose();
        _transaction = null;

        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close();
        }
        _connection.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}

