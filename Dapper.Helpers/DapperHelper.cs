using System.Data;
using System.Reflection;
using Dapper.Helpers.Interfaces;

namespace Dapper.Helpers;

/// <summary>
/// Implementação de conveniência para execução simplificada de comandos, consultas assíncronas e inserções em lote com Dapper.
/// </summary>
public class DapperHelper : IDapperHelper
{
    private readonly IDbConnection _dbConnection;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DapperHelper"/>.
    /// </summary>
    /// <param name="dbConnection">Conexão com o banco de dados.</param>
    public DapperHelper(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
    }

    /// <inheritdoc/>
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return _dbConnection.QueryAsync<T>(sql, parameters, transaction);
    }

    /// <inheritdoc/>
    public Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return _dbConnection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction);
    }

    /// <inheritdoc/>
    public Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return _dbConnection.ExecuteAsync(sql, parameters, transaction);
    }

    /// <inheritdoc/>
    public Task<int> InsertAsync<T>(string sql, T entity, IDbTransaction? transaction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(entity);
        return _dbConnection.ExecuteAsync(sql, entity, transaction);
    }

    /// <inheritdoc/>
    public Task<int> UpdateAsync<T>(string sql, T entity, IDbTransaction? transaction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        ArgumentNullException.ThrowIfNull(entity);
        return _dbConnection.ExecuteAsync(sql, entity, transaction);
    }

    /// <inheritdoc/>
    public Task<int> DeleteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        return _dbConnection.ExecuteAsync(sql, parameters, transaction);
    }

    /// <inheritdoc/>
    public async Task<int> BulkInsertAsync<T>(string tableName, IEnumerable<T> entities, int batchSize = 1000, IDbTransaction? transaction = null, bool includeId = false, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(entities);

        var list = entities.ToList();
        if (list.Count == 0) return 0;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && (p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType == typeof(byte[])))
            .Where(p => includeId || !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (properties.Count == 0)
        {
            throw new InvalidOperationException($"O tipo {typeof(T).Name} não possui propriedades legíveis para inserção.");
        }

        var columnNames = string.Join(", ", properties.Select(p => p.Name));
        var paramPlaceholders = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        var insertSql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramPlaceholders});";

        int totalAffected = 0;
        for (int i = 0; i < list.Count; i += batchSize)
        {
            ct.ThrowIfCancellationRequested();
            var batch = list.Skip(i).Take(batchSize);
            var affected = await _dbConnection.ExecuteAsync(insertSql, batch, transaction);
            totalAffected += affected;
        }

        return totalAffected;
    }
}
