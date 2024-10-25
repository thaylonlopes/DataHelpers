using System.Linq.Expressions;

namespace MongoDriver.Helpers.Interface
{
    public interface IQueryRepository<T> where T : class
    {
        IQueryable<T> Queryable { get; }

        bool Any();

        bool Any(Expression<Func<T, bool>> where);

        Task<bool> AnyAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> AnyAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default(CancellationToken));

        long Count();

        long Count(Expression<Func<T, bool>> where);

        Task<long> CountAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<long> CountAsync(Expression<Func<T, bool>> where, CancellationToken cancellationToken = default(CancellationToken));

        T Get(object key, CancellationToken cancellationToken = default(CancellationToken));

        Task<T> GetByIdAsync(object key, CancellationToken cancellationToken = default(CancellationToken));

        IEnumerable<T> List();

        Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}