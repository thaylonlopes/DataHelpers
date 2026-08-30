using MongoDriver.Helpers.Interface.Context;

namespace MongoDriver.Helpers
{
    public class MongoRepositoryBase<T> : RepositoryBase<T> where T : class
    {
        public MongoRepositoryBase(IMongoContext context)
            : base(new MongoCommandRepository<T>(context), new MongoQueryRepository<T>(context))
        {
        }
    }
}
