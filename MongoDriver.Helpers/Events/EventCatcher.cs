using MongoDriver.Helpers.Interface.Events;
using System.Collections.Concurrent;

namespace MongoDriver.Helpers.Events
{
    public sealed class EventCatcher : IEventCatcher
    {
        private readonly Lazy<ConcurrentQueue<IEvent>> _events;

        public EventCatcher() => _events = new Lazy<ConcurrentQueue<IEvent>>();

        public void Push(IEvent @event) => _events.Value.Enqueue(@event);
        public void Push(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
                _events.Value.Enqueue(@event);
        }

        public IEvent[] Pull(bool clear = false)
        {
            var all = _events.IsValueCreated ? _events.Value.ToArray() : Array.Empty<IEvent>();

            if (clear && _events.IsValueCreated)
                _events.Value.Clear();

            return all;
        }

    }
}
