using PagingFiltering.Helpers.Models;

namespace PagingFiltering.Helpers.Interfaces;

/// <summary>
/// Contrato simplificado para paginação e filtragem em coleções em memória.
/// </summary>
/// <typeparam name="T">O tipo da entidade a ser paginada.</typeparam>
public interface IPaginationHelper<T>
{
    /// <summary>
    /// Aplica paginação por deslocamento sobre a coleção.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="pageNumber">Número da página atual (1-based).</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <returns>O resultado paginado.</returns>
    PagedResult<T> ApplyPagination(IEnumerable<T> source, int pageNumber, int pageSize);

    /// <summary>
    /// Aplica filtro por correspondência de texto sobre a coleção.
    /// </summary>
    /// <param name="source">Coleção de origem.</param>
    /// <param name="filter">Termo de busca textual.</param>
    /// <returns>Coleção contendo os itens que contêm o termo.</returns>
    IEnumerable<T> ApplyFilter(IEnumerable<T> source, string filter);
}
