using PagingFiltering.Helpers.Models;
using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Interfaces;

/// <summary>
/// Contrato para serviços de filtragem, ordenação e paginação combinadas.
/// </summary>
/// <typeparam name="T">O tipo da entidade a ser manipulada.</typeparam>
public interface IPaginationFilterHelper<T>
{
    /// <summary>
    /// Aplica um predicado síncrono sobre a coleção de origem.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="filter">Função de predicado.</param>
    /// <returns>A coleção filtrada.</returns>
    IEnumerable<T> ApplyFilter(IEnumerable<T> source, Func<T, bool> filter);

    /// <summary>
    /// Aplica ordenação dinâmica por nome de propriedade sobre a coleção.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="sortBy">Nome da propriedade de ordenação.</param>
    /// <param name="ascending">Indica se a ordenação é ascendente (padrão true).</param>
    /// <returns>A coleção ordenada.</returns>
    IEnumerable<T> ApplySorting(IEnumerable<T> source, string sortBy, bool ascending = true);

    /// <summary>
    /// Aplica paginação por deslocamento (offset) sobre a coleção de origem.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="pageNumber">Número da página atual (1-based).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <returns>O resultado paginado <see cref="PagedResult{T}"/>.</returns>
    PagedResult<T> ApplyPagination(IEnumerable<T> source, int pageNumber, int pageSize);

    /// <summary>
    /// Aplica filtro assíncrono baseado em árvore de expressão linq.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="filterExpression">Expressão de predicado.</param>
    /// <returns>Coleção filtrada de forma assíncrona.</returns>
    Task<IEnumerable<T>> ApplyFilterAsync(IEnumerable<T> source, Expression<Func<T, bool>> filterExpression);

    /// <summary>
    /// Aplica paginação assíncrona por deslocamento sobre a coleção.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="pageNumber">Número da página atual (1-based).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <returns>Tarefa contendo o resultado paginado.</returns>
    Task<PagedResult<T>> ApplyPaginationAsync(IEnumerable<T> source, int pageNumber, int pageSize);

    /// <summary>
    /// Aplica paginação baseada em cursor (Keyset Seek) para navegação contínua de alto volume.
    /// </summary>
    /// <param name="source">Queryable de origem.</param>
    /// <param name="lastCursor">Valor do último cursor observado.</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <returns>Tarefa contendo o resultado com o próximo cursor.</returns>
    Task<PagedResultCursor<T>> ApplyCursorPaginationAsync(IQueryable<T> source, string lastCursor, int pageSize);

    /// <summary>
    /// Aplica múltiplos filtros complexos combinados sobre a coleção de origem.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="filters">Lista de expressões de predicado.</param>
    /// <returns>A coleção filtrada com todos os predicados aplicados.</returns>
    IEnumerable<T> ApplyComplexFilters(IEnumerable<T> source, List<Expression<Func<T, bool>>> filters);
}
