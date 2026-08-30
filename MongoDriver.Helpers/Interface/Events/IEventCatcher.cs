namespace MongoDriver.Helpers.Interface.Events
{
    public interface IEventCatcher
    {
        void Push(IEvent @event);
        void Push(IEnumerable<IEvent> events);
        IEvent[] Pull(bool clear = false);
    }
}