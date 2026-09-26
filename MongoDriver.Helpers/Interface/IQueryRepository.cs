using System.Linq.Expressions;

namespace MongoDriver.Helpers.Interface;

/// <summary>
/// Contrato para consultas e leituras tipadas em coleções do MongoDB.
/// </summary>
/// <typeparam name="T">O tipo da entidade de documento.</typeparam>
public interface IQueryRepository<T> where T : class
{
    /// <summary>
    /// Fornece acesso à consulta linq IQueryable sobre a coleção do MongoDB.
    /// </summary>
    IQueryable<T> Queryable { get; }

    /// <summary>
    /// Verifica de forma síncrona se existe algum documento na coleção.
    /// </summary>
    /// <returns><c>true</c> se houver ao menos um documento; caso contrário, <c>false</c>.</returns>
    bool Any();

    /// <summary>
    /// Verifica de forma síncrona se existe algum documento que satisfaça a condição.
    /// </summary>
    /// <param name="where">Expressão de predicado de teste.</param>
    /// <returns><c>true</c> se houver correspondência; caso contrário, <c>false</c>.</returns>
    bool Any(Expression<Func<T, bool>> where);

    /// <summary>
    /// Verifica de forma assíncrona se existe algum documento na coleção.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns><c>true</c> se houver ao menos um documento; caso contrário, <c>false</c>.</returns>
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica de forma assíncrona se existe algum documento que satisfaça o predicado.
    /// </summary>
    /// <param name="where">Expressão de predicado de teste.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns><c>true</c> se houver correspondência; caso contrário, <c>false</c>.</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna a contagem total síncrona de documentos na coleção.
    /// </summary>
    /// <returns>Quantidade total de documentos.</returns>
    long Count();

    /// <summary>
    /// Retorna a contagem síncrona de documentos que atendem a um predicado.
    /// </summary>
    /// <param name="where">Expressão de predicado para contagem.</param>
    /// <returns>Quantidade de documentos correspondentes.</returns>
    long Count(Expression<Func<T, bool>> where);

    /// <summary>
    /// Retorna a contagem total assíncrona de documentos na coleção.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Quantidade total de documentos.</returns>
    Task<long> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna a contagem assíncrona de documentos que atendem a um predicado.
    /// </summary>
    /// <param name="where">Expressão de predicado para contagem.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Quantidade de documentos correspondentes.</returns>
    Task<long> CountAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um documento pelo seu identificador síncrono.
    /// </summary>
    /// <param name="key">Chave primária ou identificador _id.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>A entidade encontrada ou null.</returns>
    T Get(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um documento pelo seu identificador de forma assíncrona.
    /// </summary>
    /// <param name="key">Chave primária ou identificador _id.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>A entidade encontrada.</returns>
    Task<T> GetByIdAsync(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna todos os documentos da coleção de forma síncrona.
    /// </summary>
    /// <returns>Coleção de documentos.</returns>
    IEnumerable<T> List();

    /// <summary>
    /// Retorna todos os documentos da coleção de forma assíncrona.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Coleção assíncrona de documentos.</returns>
    Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default);
}