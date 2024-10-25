using MongoDB.Driver;
using MongoDriver.Helpers.Interface.Context;

namespace MongoDriver.Helpers.Context
{
    public abstract class MongoContext : IMongoContext
    {
        protected MongoContext(string connectionString) => Database = new MongoClient(connectionString).GetDatabase(new MongoUrl(connectionString).DatabaseName);
        public IMongoDatabase Database { get; }
    }
}
