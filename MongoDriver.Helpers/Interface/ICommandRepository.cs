using System.Linq.Expressions;
using MongoDriver.Helpers.Models;

namespace MongoDriver.Helpers.Interface;

/// <summary>
/// Contrato de comandos de persistência (escrita e paginação) para coleções de documentos no MongoDB.
/// </summary>
/// <typeparam name="T">O tipo da entidade de documento.</typeparam>
public interface ICommandRepository<T> where T : class
{
    /// <summary>
    /// Adiciona um documento na coleção de forma síncrona.
    /// </summary>
    /// <param name="item">O documento a ser inserido.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void Add(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona um documento na coleção de forma assíncrona.
    /// </summary>
    /// <param name="item">O documento a ser inserido.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da inserção.</returns>
    Task AddAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere uma coleção de documentos de forma síncrona.
    /// </summary>
    /// <param name="items">Coleção de documentos a inserir.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere uma coleção de documentos de forma assíncrona.
    /// </summary>
    /// <param name="items">Coleção de documentos a inserir.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da inserção em lote.</returns>
    Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui um documento pelo seu identificador de forma síncrona.
    /// </summary>
    /// <param name="key">A chave primária ou _id do documento.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void Delete(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui documentos correspondentes a um predicado de forma síncrona.
    /// </summary>
    /// <param name="where">Expressão de filtro para os documentos a excluir.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui um documento pelo seu identificador de forma assíncrona.
    /// </summary>
    /// <param name="key">A chave primária ou _id do documento.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da exclusão.</returns>
    Task DeleteAsync(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui documentos correspondentes a um predicado de forma assíncrona.
    /// </summary>
    /// <param name="where">Expressão de filtro para os documentos a excluir.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da exclusão.</returns>
    Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

    /// <summary>
    /// Substitui ou atualiza um documento completo de forma síncrona.
    /// </summary>
    /// <param name="item">A entidade com o conteúdo atualizado.</param>
    void Update(T item);

    /// <summary>
    /// Substitui ou atualiza um documento completo de forma assíncrona.
    /// </summary>
    /// <param name="item">A entidade com o conteúdo atualizado.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da atualização.</returns>
    Task UpdateAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza campos parciais de um documento de forma síncrona.
    /// </summary>
    /// <param name="item">Objeto contendo o identificador e os campos modificados.</param>
    void UpdatePartial(object item);

    /// <summary>
    /// Atualiza campos parciais de um documento de forma assíncrona.
    /// </summary>
    /// <param name="item">Objeto contendo o identificador e os campos modificados.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da atualização parcial.</returns>
    Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza múltiplos documentos de forma síncrona.
    /// </summary>
    /// <param name="items">Coleção de documentos a atualizar.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza múltiplos documentos de forma assíncrona.
    /// </summary>
    /// <param name="items">Coleção de documentos a atualizar.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da atualização em lote.</returns>
    Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de documentos com filtro e ordenação de forma assíncrona.
    /// </summary>
    /// <param name="filter">Expressão de filtro de documentos.</param>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de documentos por página.</param>
    /// <param name="sortField">Campo de ordenação.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Resultado paginado contendo os documentos e totalizador.</returns>
    Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de documentos sem filtro prévio com ordenação de forma assíncrona.
    /// </summary>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de documentos por página.</param>
    /// <param name="sortField">Campo de ordenação.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Resultado paginado contendo os documentos e totalizador.</returns>
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de documentos com ordenações compostas múltiplas de forma assíncrona.
    /// </summary>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de documentos por página.</param>
    /// <param name="sortDefinitions">Definições compostas de ordenação.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Resultado paginado contendo os documentos correspondentes.</returns>
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de documentos com filtro e ordenações compostas múltiplas de forma assíncrona.
    /// </summary>
    /// <param name="filter">Expressão de filtro de documentos.</param>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de documentos por página.</param>
    /// <param name="sortDefinitions">Definições compostas de ordenação.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Resultado paginado contendo os documentos correspondentes.</returns>
    Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default);
}