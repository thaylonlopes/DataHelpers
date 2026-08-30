using Dapper.Helpers.Models;
using System.Linq.Expressions;

namespace Dapper.Helpers.Interfaces
{
    public interface ICommandRepository<T> where T : class
    {
        void Add(T item, CancellationToken cancellationToken = default);
        Task AddAsync(T item, CancellationToken cancellationToken = default);
        void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);
        void Delete(object key, CancellationToken cancellationToken = default);
        void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);
        Task DeleteAsync(object key, CancellationToken cancellationToken = default);
        Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default);
        void Update(T item);
        Task UpdateAsync(T item, CancellationToken cancellationToken = default);
        void UpdatePartial(object item);
        Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default);
        void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);
        Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default);
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default);
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default);
        Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default);
    }
}
