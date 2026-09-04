
using PagingFiltering.Helpers.Interfaces;
using PagingFiltering.Helpers.Models;
using System.Linq.Expressions;

namespace PagingFiltering.Helpers
{
    public class PaginationFilterHelper<T> : IPaginationFilterHelper<T>
    {
        private readonly ICacheService _cacheService;

        public PaginationFilterHelper(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public IEnumerable<T> ApplyFilter(IEnumerable<T> source, Func<T, bool> filter)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(filter);

            return source.Where(filter);
        }

        public IEnumerable<T> ApplySorting(IEnumerable<T> source, string sortBy, bool ascending = true)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(sortBy);

            var param = Expression.Parameter(typeof(T));
            var property = Expression.Property(param, sortBy);
            var sortExpression = Expression.Lambda<Func<T, object>>(Expression.Convert(property, typeof(object)), param);

            return ascending ? source.AsQueryable().OrderBy(sortExpression) : source.AsQueryable().OrderByDescending(sortExpression);
        }

        public PagedResult<T> ApplyPagination(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source);

            var totalItems = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<T>(items, pageNumber, pageSize, totalItems);
        }

        public Task<PagedResult<T>> ApplyPaginationAsync(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source);

            var totalItems = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Task.FromResult(new PagedResult<T>(items, pageNumber, pageSize, totalItems));
        }

        public async Task<IEnumerable<T>> ApplyCachingAsync(string cacheKey, Func<Task<IEnumerable<T>>> getDataFunc)
        {
            ArgumentNullException.ThrowIfNull(cacheKey);
            ArgumentNullException.ThrowIfNull(getDataFunc);

            var cachedData = await _cacheService.GetAsync<IEnumerable<T>>(cacheKey);
            if (cachedData == null)
            {
                cachedData = await getDataFunc();
                await _cacheService.SetCachedDataAsync(cacheKey, cachedData);
            }
            return cachedData;
        }

        public async Task<PagedResult<T>> ApplyPaginationCachedAsync(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source);

            var cacheKey = $"PagedResult_{pageNumber}_{pageSize}";
            var cachedResult = await _cacheService.GetAsync<PagedResult<T>>(cacheKey);

            if (cachedResult != null)
            {
                return cachedResult;
            }

            var totalItems = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var pagedResult = new PagedResult<T>(items, pageNumber, pageSize, totalItems);

            await _cacheService.SetAsync(cacheKey, pagedResult, TimeSpan.FromMinutes(10));
            return pagedResult;
        }

        public async Task<IEnumerable<T>> ApplyFilterAsync(IEnumerable<T> source, Expression<Func<T, bool>> filterExpression)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(filterExpression);

            return await Task.Run(() => source.AsQueryable().Where(filterExpression).ToList());
        }

        public Task<PagedResultCursor<T>> ApplyCursorPaginationAsync(IQueryable<T> source, string lastCursor, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(lastCursor);

            var propertyInfo = typeof(T).GetProperty("Id");
            if (propertyInfo == null)
            {
                throw new InvalidOperationException("A classe T não possui uma propriedade 'Id'.");
            }

            var filteredSource = source.AsQueryable()
                .Where(item => string.Compare(propertyInfo.GetValue(item) as string ?? string.Empty, lastCursor, StringComparison.Ordinal) > 0)
                .OrderBy(item => propertyInfo.GetValue(item) as string ?? string.Empty);

            var totalItems = source.Count();
            var items = source.Take(pageSize).ToList();

            var lastItem = items.LastOrDefault();
            var newCursor = propertyInfo.GetValue(lastItem) as string ?? string.Empty;

            return Task.FromResult(new PagedResultCursor<T>(items, pageSize, totalItems, newCursor));
        }

        public IEnumerable<T> ApplyComplexFilters(IEnumerable<T> source, List<Expression<Func<T, bool>>> filters)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(filters);

            var query = source.AsQueryable();
            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }
            return query.ToList();
        }

    }
}
