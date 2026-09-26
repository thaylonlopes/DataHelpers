using PagingFiltering.Helpers.Interfaces;
using PagingFiltering.Helpers.Models;

namespace PagingFiltering.Helpers.Implementations;

/// <summary>
/// Utilitário simplificado para paginação em memória e filtragem por texto.
/// </summary>
/// <typeparam name="T">O tipo da entidade a ser paginada.</typeparam>
public class PaginationHelper<T> : IPaginationHelper<T>
{
    /// <inheritdoc />
    public PagedResult<T> ApplyPagination(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        var totalItems = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<T>(items, pageNumber, pageSize, totalItems);
    }

    /// <inheritdoc />
    public IEnumerable<T> ApplyFilter(IEnumerable<T> source, string filter)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filter);

        return source.Where(item => (item?.ToString() ?? string.Empty).Contains(filter));
    }
}
