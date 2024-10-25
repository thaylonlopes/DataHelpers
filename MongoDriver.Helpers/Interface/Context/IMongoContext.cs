using MongoDB.Driver;

namespace MongoDriver.Helpers.Interface.Context
{
    public interface IMongoContext
    {
        IMongoDatabase Database { get; }
    }
}
