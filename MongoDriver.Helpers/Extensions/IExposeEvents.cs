using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Extensions
{
    public interface IExposeEvents
    {
        IEnumerable<IEvent> Events { get; }
        void ClearEvents();
    }
}