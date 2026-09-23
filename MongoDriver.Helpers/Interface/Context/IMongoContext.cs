using MongoDB.Driver;
using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Interface.Context
{
    public interface IMongoContext : IDisposable
    {
        IEventCatcher EventCatcher { get; }
        IClientSessionHandle? ClientSession { get; }
        bool IsInTransaction { get; }

        IMongoCollection<T> GetCollection<T>(string name);
        Task AddCommand(Func<Task> func);
        Task RemoveCommand(Func<Task> func);
        Task<int> SaveChanges();

        Task<IClientSessionHandle> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
