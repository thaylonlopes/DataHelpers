using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Interceptor
{
    public class SaveChangesInterceptor
    {
        public virtual Task<bool> BeforeCommitsAsync(IEventCatcher eventCatcher)
        {
            return Task.FromResult(true);
        }
    }
}
