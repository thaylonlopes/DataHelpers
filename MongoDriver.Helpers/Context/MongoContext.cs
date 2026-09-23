using MongoDB.Driver;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Context;

public class MongoContext : IMongoContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;
    private readonly List<Func<Task>> _tasks;
    public IClientSessionHandle? ClientSession { get; private set; }

    public bool IsInTransaction => ClientSession?.IsInTransaction ?? false;

    public IEventCatcher EventCatcher { get; }

    public MongoContext(IMongoClientDatabase clientDatabase, IEventCatcher eventCatcher)
    {
        ArgumentNullException.ThrowIfNull(clientDatabase);
        ArgumentNullException.ThrowIfNull(eventCatcher);

        _database = clientDatabase.Database;
        _tasks = new List<Func<Task>>();
        _client = clientDatabase.DatabaseClient;
        EventCatcher = eventCatcher;
    }

    ~MongoContext() => Dispose();

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _database.GetCollection<T>(name);
    }

    public Task AddCommand(Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _tasks.Add(func);
        return Task.CompletedTask;
    }

    public Task RemoveCommand(Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _tasks.Remove(func);
        return Task.CompletedTask;
    }

    public async Task<IClientSessionHandle> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (ClientSession == null)
        {
            ClientSession = await _client.StartSessionAsync(cancellationToken: cancellationToken);
        }

        if (ClientSession != null && !ClientSession.IsInTransaction)
        {
            ClientSession.StartTransaction();
        }

        return ClientSession!;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (ClientSession != null && ClientSession.IsInTransaction)
        {
            await ClientSession.CommitTransactionAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (ClientSession != null && ClientSession.IsInTransaction)
        {
            await ClientSession.AbortTransactionAsync(cancellationToken);
        }
    }

    public async Task<int> SaveChanges()
    {
        var shouldDisposeSession = false;
        if (ClientSession == null || !ClientSession.IsInTransaction)
        {
            ClientSession = await _client.StartSessionAsync();
            ClientSession.StartTransaction();
            shouldDisposeSession = true;
        }

        try
        {
            var commands = _tasks.Select(t => t());
            await Task.WhenAll(commands);

            if (shouldDisposeSession)
            {
                await ClientSession.CommitTransactionAsync();
            }

            var count = _tasks.Count;
            _tasks.Clear();
            return count;
        }
        catch
        {
            if (ClientSession != null && ClientSession.IsInTransaction)
            {
                await ClientSession.AbortTransactionAsync();
            }
            throw;
        }
        finally
        {
            if (shouldDisposeSession)
            {
                ClientSession?.Dispose();
                ClientSession = null;
            }
        }
    }

    public void Dispose()
    {
        ClientSession?.Dispose();
        GC.SuppressFinalize(this);
    }
}
