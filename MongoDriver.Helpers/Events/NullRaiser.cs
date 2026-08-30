using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Events
{
    internal class NullRaiser : IRaiser
    {
        public Task RaiseAsync(IEvent @event) => Task.CompletedTask;

        public Task RaiseAsync(IEnumerable<IEvent> events) => Task.CompletedTask;
    }
}
