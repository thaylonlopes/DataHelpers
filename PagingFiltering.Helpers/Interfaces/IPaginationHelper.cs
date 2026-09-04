using PagingFiltering.Helpers.Models;

namespace PagingFiltering.Helpers.Interfaces
{
    public interface IPaginationHelper<T>
    {
        PagedResult<T> ApplyPagination(IEnumerable<T> source, int pageNumber, int pageSize);
        IEnumerable<T> ApplyFilter(IEnumerable<T> source, string filter);
    }
}
