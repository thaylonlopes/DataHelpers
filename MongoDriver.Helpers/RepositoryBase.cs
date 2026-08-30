using System.Linq.Expressions;
using MongoDriver.Helpers.Interface;
using MongoDriver.Helpers.Models;

namespace MongoDriver.Helpers;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly ICommandRepository<T> _commandRepository;
    private readonly IQueryRepository<T> _queryRepository;

    protected RepositoryBase
    (
        ICommandRepository<T> commandRepository,
        IQueryRepository<T> queryRepository
    )
    {
        _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
    }

    public IQueryable<T> Queryable => _queryRepository.Queryable;

    public void Add(T item, CancellationToken cancellationToken = default) => _commandRepository.Add(item, cancellationToken);

    public Task AddAsync(T item, CancellationToken cancellationToken = default) => _commandRepository.AddAsync(item, cancellationToken);

    public void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.AddRange(items, cancellationToken);

    public Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.AddRangeAsync(items, cancellationToken);

    public bool Any() => _queryRepository.Any();

    public bool Any(Expression<Func<T, bool>> where) => _queryRepository.Any(where);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => _queryRepository.AnyAsync(cancellationToken);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _queryRepository.AnyAsync(where, cancellationToken);

    public long Count() => _queryRepository.Count();

    public long Count(Expression<Func<T, bool>> where) => _queryRepository.Count(where);

    public Task<long> CountAsync(CancellationToken cancellationToken = default) => _queryRepository.CountAsync(cancellationToken);

    public Task<long> CountAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _queryRepository.CountAsync(where, cancellationToken);

    public void Delete(object key, CancellationToken cancellationToken = default) => _commandRepository.Delete(key, cancellationToken);

    public void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _commandRepository.Delete(where, cancellationToken);

    public Task DeleteAsync(object key, CancellationToken cancellationToken = default) => _commandRepository.DeleteAsync(key, cancellationToken);

    public Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _commandRepository.DeleteAsync(where, cancellationToken);

    public Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(filter, pageNumber, pageSize, sortField, ascending, cancellationToken);

    public Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(pageNumber, pageSize, sortField, ascending, cancellationToken);

    public Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(pageNumber, pageSize, sortDefinitions, cancellationToken);

    public Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(filter, pageNumber, pageSize, sortDefinitions, cancellationToken);

    public T Get(object key, CancellationToken cancellationToken = default) => _queryRepository.Get(key, cancellationToken);

    public Task<T> GetByIdAsync(object key, CancellationToken cancellationToken = default) => _queryRepository.GetByIdAsync(key, cancellationToken);

    public IEnumerable<T> List() => _queryRepository.List();

    public Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default) => _queryRepository.ListAsync(cancellationToken);

    public void Update(T item) => _commandRepository.Update(item);

    public Task UpdateAsync(T item, CancellationToken cancellationToken = default) => _commandRepository.UpdateAsync(item, cancellationToken);

    public void UpdatePartial(object item) => _commandRepository.UpdatePartial(item);

    public Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default) => _commandRepository.UpdatePartialAsync(item, cancellationToken);

    public void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.UpdateRange(items, cancellationToken);

    public Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.UpdateRangeAsync(items, cancellationToken);
}
