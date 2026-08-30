using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Interceptor
{
    public class CaptureEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IRaiser _raiser;

        public CaptureEventsInterceptor(IRaiser raiser)
        {
            _raiser = raiser;
        }

        public override async Task<bool> BeforeCommitsAsync(IEventCatcher eventCatcher)
        {
            await _raiser.RaiseAsync(eventCatcher.Pull());
            return true;
        }
    }
}
