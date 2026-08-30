using MongoDriver.Helpers.Enum;

namespace MongoDriver.Helpers.Interface.Events
{
    public interface IEvent
    {
        MessageType Type { get; }
    }
}
