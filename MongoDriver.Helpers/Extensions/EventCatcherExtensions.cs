using MongoDriver.Helpers.Interface.Document;
using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Extensions
{
    public static class EventCatcherExtensions
    {
        public static void PushAll(this IEventCatcher eventCatcher, IDocument entity)
        {
            if (entity is IExposeEvents exposeEvents)
                eventCatcher.Push(exposeEvents.Events);
        }
    }
}
