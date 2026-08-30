using MongoDB.Driver;
using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Interface.Context
{
    public interface IMongoContext : IDisposable
    {
        IEventCatcher EventCatcher { get; }
        IMongoCollection<T> GetCollection<T>(string name);
        Task AddCommand(Func<Task> func);
        Task RemoveCommand(Func<Task> func);
        Task<int> SaveChanges();
    }
}
