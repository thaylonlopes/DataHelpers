using System.Linq.Expressions;
using MongoDriver.Helpers.Interface;
using MongoDriver.Helpers.Models;

namespace MongoDriver.Helpers;

/// <summary>
/// Classe base abstrata para repositórios CQRS no MongoDB encapsulando command e query repositories.
/// </summary>
/// <typeparam name="T">O tipo da entidade de documento.</typeparam>
public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly ICommandRepository<T> _commandRepository;
    private readonly IQueryRepository<T> _queryRepository;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="RepositoryBase{T}"/> injetando os repositórios de comando e consulta.
    /// </summary>
    /// <param name="commandRepository">Repositório responsável pelas mutações e comandos.</param>
    /// <param name="queryRepository">Repositório responsável por consultas e leituras.</param>
    protected RepositoryBase
    (
        ICommandRepository<T> commandRepository,
        IQueryRepository<T> queryRepository
    )
    {
        _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
        _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
    }

    /// <inheritdoc/>
    public IQueryable<T> Queryable => _queryRepository.Queryable;

    /// <inheritdoc/>
    public void Add(T item, CancellationToken cancellationToken = default) => _commandRepository.Add(item, cancellationToken);

    /// <inheritdoc/>
    public Task AddAsync(T item, CancellationToken cancellationToken = default) => _commandRepository.AddAsync(item, cancellationToken);

    /// <inheritdoc/>
    public void AddRange(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.AddRange(items, cancellationToken);

    /// <inheritdoc/>
    public Task AddRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.AddRangeAsync(items, cancellationToken);

    /// <inheritdoc/>
    public bool Any() => _queryRepository.Any();

    /// <inheritdoc/>
    public bool Any(Expression<Func<T, bool>> where) => _queryRepository.Any(where);

    /// <inheritdoc/>
    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => _queryRepository.AnyAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<bool> AnyAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _queryRepository.AnyAsync(where, cancellationToken);

    /// <inheritdoc/>
    public long Count() => _queryRepository.Count();

    /// <inheritdoc/>
    public long Count(Expression<Func<T, bool>> where) => _queryRepository.Count(where);

    /// <inheritdoc/>
    public Task<long> CountAsync(CancellationToken cancellationToken = default) => _queryRepository.CountAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<long> CountAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _queryRepository.CountAsync(where, cancellationToken);

    /// <inheritdoc/>
    public void Delete(object key, CancellationToken cancellationToken = default) => _commandRepository.Delete(key, cancellationToken);

    /// <inheritdoc/>
    public void Delete(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _commandRepository.Delete(where, cancellationToken);

    /// <inheritdoc/>
    public Task DeleteAsync(object key, CancellationToken cancellationToken = default) => _commandRepository.DeleteAsync(key, cancellationToken);

    /// <inheritdoc/>
    public Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default) => _commandRepository.DeleteAsync(where, cancellationToken);

    /// <inheritdoc/>
    public Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(filter, pageNumber, pageSize, sortField, ascending, cancellationToken);

    /// <inheritdoc/>
    public Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, string? sortField = null, bool ascending = true, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(pageNumber, pageSize, sortField, ascending, cancellationToken);

    /// <inheritdoc/>
    public Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(pageNumber, pageSize, sortDefinitions, cancellationToken);

    /// <inheritdoc/>
    public Task<PagedResult<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize, List<SortDefinition>? sortDefinitions = null, CancellationToken cancellationToken = default) =>
        _commandRepository.GetPagedAsync(filter, pageNumber, pageSize, sortDefinitions, cancellationToken);

    /// <inheritdoc/>
    public T Get(object key, CancellationToken cancellationToken = default) => _queryRepository.Get(key, cancellationToken);

    /// <inheritdoc/>
    public Task<T> GetByIdAsync(object key, CancellationToken cancellationToken = default) => _queryRepository.GetByIdAsync(key, cancellationToken);

    /// <inheritdoc/>
    public IEnumerable<T> List() => _queryRepository.List();

    /// <inheritdoc/>
    public Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default) => _queryRepository.ListAsync(cancellationToken);

    /// <inheritdoc/>
    public void Update(T item) => _commandRepository.Update(item);

    /// <inheritdoc/>
    public Task UpdateAsync(T item, CancellationToken cancellationToken = default) => _commandRepository.UpdateAsync(item, cancellationToken);

    /// <inheritdoc/>
    public void UpdatePartial(object item) => _commandRepository.UpdatePartial(item);

    /// <inheritdoc/>
    public Task UpdatePartialAsync(object item, CancellationToken cancellationToken = default) => _commandRepository.UpdatePartialAsync(item, cancellationToken);

    /// <inheritdoc/>
    public void UpdateRange(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.UpdateRange(items, cancellationToken);

    /// <inheritdoc/>
    public Task UpdateRangeAsync(IEnumerable<T> items, CancellationToken cancellationToken = default) => _commandRepository.UpdateRangeAsync(items, cancellationToken);
}
