using PagingFiltering.Helpers.Models;
using System.Linq.Expressions;

namespace PagingFiltering.Helpers.Interfaces
{
    public interface IPaginationFilterHelper<T>
    {
        IEnumerable<T> ApplyFilter(IEnumerable<T> source, Func<T, bool> filter);
        IEnumerable<T> ApplySorting(IEnumerable<T> source, string sortBy, bool ascending = true);
        PagedResult<T> ApplyPagination(IEnumerable<T> source, int pageNumber, int pageSize);
        Task<IEnumerable<T>> ApplyFilterAsync(IEnumerable<T> source, Expression<Func<T, bool>> filterExpression);
        Task<PagedResult<T>> ApplyPaginationAsync(IEnumerable<T> source, int pageNumber, int pageSize);
        Task<IEnumerable<T>> ApplyCachingAsync(string cacheKey, Func<Task<IEnumerable<T>>> getDataFunc);
        Task<PagedResult<T>> ApplyPaginationCachedAsync(IEnumerable<T> source, int pageNumber, int pageSize);
        Task<PagedResultCursor<T>> ApplyCursorPaginationAsync(IQueryable<T> source, string lastCursor, int pageSize);
        IEnumerable<T> ApplyComplexFilters(IEnumerable<T> source, List<Expression<Func<T, bool>>> filters);
    }
}
