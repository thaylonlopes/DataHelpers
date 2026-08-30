using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDriver.Helpers.Config;
using MongoDriver.Helpers.Interface.Context;

namespace MongoDriver.Helpers.Context
{
    public class MongoClientDatabase : IMongoClientDatabase
    {
        public MongoClientDatabase(IOptions<MongoDbConfig> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            DatabaseClient = new MongoClient(options.Value.ConnectionStrings);
            Database = DatabaseClient.GetDatabase(options.Value.Database);
        }
        public IMongoDatabase Database { get; }
        public IMongoClient DatabaseClient { get; }
    }
}