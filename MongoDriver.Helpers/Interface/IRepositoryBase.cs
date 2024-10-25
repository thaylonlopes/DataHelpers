namespace MongoDriver.Helpers.Interface
{
    public interface IRepositoryBase<T> : ICommandRepository<T>, IQueryRepository<T> where T : class { }
}