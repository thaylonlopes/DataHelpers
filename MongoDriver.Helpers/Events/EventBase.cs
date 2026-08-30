using MongoDriver.Helpers.Enum;
using MongoDriver.Helpers.Interface.Events;

namespace MongoDriver.Helpers.Events
{
    public class EventBase : IEvent
    {
        public MessageType Type => MessageType.Event;
    }
}
