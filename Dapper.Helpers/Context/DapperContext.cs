using System.Data;
using Dapper.Helpers.Interfaces;
using Microsoft.Data.SqlClient;

namespace Dapper.Helpers.Context;

/// <summary>
/// Contexto base para conexões SQL via ADO.NET / Dapper.
/// </summary>
public abstract class DapperContext : IDbContext
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="DapperContext"/> com a string de conexão informada.
    /// </summary>
    /// <param name="connectionString">String de conexão com o banco de dados.</param>
    protected DapperContext(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        Connection = new SqlConnection(connectionString);
    }

    /// <inheritdoc/>
    public IDbConnection Connection { get; }
}
