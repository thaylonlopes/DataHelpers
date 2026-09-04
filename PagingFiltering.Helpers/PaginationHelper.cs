using PagingFiltering.Helpers.Interfaces;
using PagingFiltering.Helpers.Models;

namespace PagingFiltering.Helpers.Implementations
{
    public class PaginationHelper<T> : IPaginationHelper<T>
    {
        public PagedResult<T> ApplyPagination(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var totalItems = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<T>(items, pageNumber, pageSize, totalItems);
        }

        public IEnumerable<T> ApplyFilter(IEnumerable<T> source, string filter)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(filter);

            return source.Where(item => (item?.ToString() ?? string.Empty).Contains(filter));
        }
    }
}
