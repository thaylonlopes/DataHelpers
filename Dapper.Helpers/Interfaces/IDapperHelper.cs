using System.Data;

namespace Dapper.Helpers.Interfaces;

/// <summary>
/// Contrato de conveniência para execução simplificada de comandos e consultas assíncronas com Dapper e suporte a transações.
/// </summary>
public interface IDapperHelper
{
    /// <summary>
    /// Executa uma consulta assíncrona retornando uma coleção tipada.
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null);

    /// <summary>
    /// Executa uma consulta assíncrona retornando um único registro ou valor padrão.
    /// </summary>
    Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, IDbTransaction? transaction = null);

    /// <summary>
    /// Executa um comando SQL assíncrono retornando a quantidade de linhas afetadas.
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null);

    /// <summary>
    /// Insere uma entidade no banco de dados executando o comando SQL assíncrono.
    /// </summary>
    Task<int> InsertAsync<T>(string sql, T entity, IDbTransaction? transaction = null);

    /// <summary>
    /// Atualiza uma entidade no banco de dados executando o comando SQL assíncrono.
    /// </summary>
    Task<int> UpdateAsync<T>(string sql, T entity, IDbTransaction? transaction = null);

    /// <summary>
    /// Executa um comando de deleção SQL assíncrono.
    /// </summary>
    Task<int> DeleteAsync(string sql, object? parameters = null, IDbTransaction? transaction = null);

    /// <summary>
    /// Executa inserção em lote de múltiplos registros com alto desempenho e controle transacional.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade.</typeparam>
    /// <param name="tableName">Nome da tabela de destino.</param>
    /// <param name="entities">A coleção de entidades a ser inserida.</param>
    /// <param name="batchSize">Tamanho do lote de inserção (padrão 1000).</param>
    /// <param name="transaction">Transação ativa opcional.</param>
    /// <param name="includeId">Se verdadeiro, inclui a propriedade Id no INSERT (padrão falso para colunas auto-incremento).</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>A quantidade total de registros inseridos.</returns>
    Task<int> BulkInsertAsync<T>(string tableName, IEnumerable<T> entities, int batchSize = 1000, IDbTransaction? transaction = null, bool includeId = false, CancellationToken ct = default);
}
