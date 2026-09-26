using Dapper.Helpers.Models;
using System.Linq.Expressions;

namespace Dapper.Helpers.Interfaces;

/// <summary>
/// Contrato genérico para repositórios de comando (escrita e paginação) via Micro-ORM Dapper.
/// </summary>
/// <typeparam name="T">O tipo da entidade gerenciada.</typeparam>
public interface ICommandRepository<T> where T : class
{
    /// <summary>
    /// Adiciona uma entidade de forma síncrona.
    /// </summary>
    /// <param name="item">A entidade a ser adicionada.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void Add(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma entidade de forma assíncrona.
    /// </summary>
    /// <param name="item">A entidade a ser adicionada.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da operação.</returns>
    Task AddAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma coleção de entidades de forma síncrona.
    /// </summary>
    /// <param name="items">Coleção de entidades a serem inseridas.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma coleção de entidades de forma assíncrona.
    /// </summary>
    /// <param name="items">Coleção de entidades a serem inseridas.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da operação.</returns>
    Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui uma entidade pela sua chave primária de forma síncrona.
    /// </summary>
    /// <param name="key">O valor da chave identificadora.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void Delete(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui entidades correspondentes a uma expressão de predicado de forma síncrona.
    /// </summary>
    /// <param name="where">Expressão booleana para filtragem dos registros a excluir.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui uma entidade pela sua chave primária de forma assíncrona.
    /// </summary>
    /// <param name="key">O valor da chave identificadora.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da exclusão.</returns>
    Task DeleteAsync(object key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exclui entidades correspondentes a um predicado de forma assíncrona.
    /// </summary>
    /// <param name="where">Expressão booleana para filtragem dos registros a excluir.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da exclusão.</returns>
    Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma entidade existente de forma síncrona.
    /// </summary>
    /// <param name="item">A entidade com os valores atualizados.</param>
    void Update(T item);

    /// <summary>
    /// Atualiza uma entidade existente de forma assíncrona.
    /// </summary>
    /// <param name="item">A entidade com os valores atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da atualização.</returns>
    Task UpdateAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza a atualização parcial de propriedades de uma entidade de forma síncrona.
    /// </summary>
    /// <param name="item">Objeto contendo a chave primária Id e os campos a atualizar.</param>
    void UpdatePartial(object item);

    /// <summary>
    /// Realiza a atualização parcial de propriedades de uma entidade de forma assíncrona.
    /// </summary>
    /// <param name="item">Objeto contendo a chave primária Id e os campos a atualizar.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da atualização parcial.</returns>
    Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma lista de entidades de forma síncrona.
    /// </summary>
    /// <param name="items">Coleção de entidades a atualizar.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma lista de entidades de forma assíncrona.
    /// </summary>
    /// <param name="items">Coleção de entidades a atualizar.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da atualização.</returns>
    Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de resultados com filtro, paginação e ordenação por campo de forma assíncrona.
    /// </summary>
    /// <param name="filter">Expressão de filtro dos registros.</param>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <param name="sortField">Nome do campo para ordenação.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Um resultado paginado contendo a lista de itens e metadados.</returns>
    Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de resultados sem filtro prévio com ordenação por campo de forma assíncrona.
    /// </summary>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <param name="sortField">Nome do campo para ordenação.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Um resultado paginado contendo os itens e totalizador.</returns>
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de resultados com ordenações compostas múltiplas de forma assíncrona.
    /// </summary>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <param name="sortDefinitions">Lista de definições de ordenação por campo e direção.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Um resultado paginado contendo os itens correspondentes.</returns>
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma página de resultados com filtro e ordenações compostas múltiplas de forma assíncrona.
    /// </summary>
    /// <param name="filter">Expressão de filtro dos registros.</param>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <param name="sortDefinitions">Lista de definições de ordenação por campo e direção.</param>
    /// <param name="cancellationToken">Token de cancelamento cooperativo.</param>
    /// <returns>Um resultado paginado contendo os itens correspondentes.</returns>
    Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default);
}
