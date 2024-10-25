using MongoDriver.Helpers.Interface.Context;

namespace MongoDriver.Helpers
{
    public class MongoRepositoryBase<T>(IMongoContext context) : RepositoryBase<T>(new MongoCommandRepository<T>(context), new MongoQueryRepository<T>(context)) where T : class;
}
