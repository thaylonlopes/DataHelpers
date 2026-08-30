namespace MongoDriver.Helpers.Interface.Events
{
    public interface IEventHandler<in TEvent> where TEvent : IEvent
    {
        Task HandleAsync(IEvent @event);
    }
}
