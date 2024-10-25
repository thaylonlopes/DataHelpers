using MongoDriver.Helpers.Models;
using System.Linq.Expressions;

namespace MongoDriver.Helpers.Interface
{
    public interface ICommandRepository<T> where T : class
    {
        void Add(T item, CancellationToken cancellationToken = default(CancellationToken));

        Task AddAsync(T item, CancellationToken cancellationToken = default(CancellationToken));

        void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default(CancellationToken));

        Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default(CancellationToken));

        void Delete(object key, CancellationToken cancellationToken = default(CancellationToken));

        void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteAsync(object key, CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default(CancellationToken));

        void Update(T item);

        Task UpdateAsync(T item, CancellationToken cancellationToken = default(CancellationToken));

        void UpdatePartial(object item);

        Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default(CancellationToken));

        void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default(CancellationToken));

        Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default(CancellationToken));
        Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string sortField = null, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken));
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string sortField = null, bool ascending = true, CancellationToken cancellationToken = default(CancellationToken));
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition> sortDefinitions = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition> sortDefinitions = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}