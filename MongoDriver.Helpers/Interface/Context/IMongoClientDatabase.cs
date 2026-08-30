using MongoDB.Driver;

namespace MongoDriver.Helpers.Interface.Context
{
    public interface IMongoClientDatabase
    {
        IMongoDatabase Database { get; }
        IMongoClient DatabaseClient { get; }
    }
}