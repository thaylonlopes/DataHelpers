namespace MongoDriver.Helpers.Interface.Events
{
    public interface IRaiser
    {
        Task RaiseAsync(IEvent @event);
        Task RaiseAsync(IEnumerable<IEvent> events);

    }
}
