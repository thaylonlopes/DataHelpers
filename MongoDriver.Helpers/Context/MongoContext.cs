using MongoDB.Driver;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Context;

public class MongoContext : IMongoContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;
    private readonly ICollection<Func<Task>> _tasks;
    public IClientSessionHandle? ClientSession { get; private set; }

    public IEventCatcher EventCatcher { get; }

    public MongoContext(IMongoClientDatabase clientDatabase, IEventCatcher eventCatcher)
    {
        if (clientDatabase == null) throw new ArgumentNullException(nameof(clientDatabase));
        if (eventCatcher == null) throw new ArgumentNullException(nameof(eventCatcher));
        _database = clientDatabase.Database;
        _tasks = new List<Func<Task>>();
        _client = clientDatabase.DatabaseClient;
        EventCatcher = eventCatcher;
    }

    ~MongoContext() => Dispose();

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        return _database.GetCollection<T>(name);
    }

    public Task AddCommand(Func<Task> func)
    {
        _tasks.Add(func);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChanges()
    {
        using (ClientSession = await _client.StartSessionAsync())
        {
            ClientSession.StartTransaction();
            var commands = _tasks.Select(t => t());
            await Task.WhenAll(commands);
            await ClientSession.CommitTransactionAsync();
        }
        return _tasks.Count;
    }

    public void Dispose()
    {
        ClientSession?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task RemoveCommand(Func<Task> func)
    {
        if (_tasks.Contains(func))
        {
            _tasks.Remove(func);
        }
        return Task.CompletedTask;
    }
}
