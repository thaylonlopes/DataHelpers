using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using MongoDriver.Helpers.Constants;
using MongoDriver.Helpers.Interface.Events;
using MongoDriver.Helpers.Models;

namespace MongoDriver.Helpers.Events;

public class Raiser : IRaiser
{
    private static readonly ConcurrentDictionary<string, EventBinder> _handlersType = new ConcurrentDictionary<string, EventBinder>();
    private readonly IServiceProvider _serviceProvider;

    public Raiser(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task RaiseAsync(IEvent @event)
    {
        if (@event == null) return;
        var eventType = @event.GetType();
        var eventBinder = _handlersType.GetOrAdd(eventType.Name, x =>
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var method = handlerType.GetMethod(EventhandlerConstants.HandlerName, new[] { eventType })
                ?? throw new InvalidOperationException($"Method '{EventhandlerConstants.HandlerName}' not found on type '{handlerType.FullName}'.");
            return new EventBinder(handlerType, method);
        });

        var eventHandlers = _serviceProvider.GetServices(eventBinder.HandlerType);
        if (eventHandlers is null || !eventHandlers.Any()) return;

        var handlers = eventHandlers
            .Select(e => eventBinder.HandlerMethod.Invoke(e, new object[] { @event }))
            .OfType<Task>();

        await Task.WhenAll(handlers);
    }

    public async Task RaiseAsync(IEnumerable<IEvent> events)
    {
        if (events == null) return;
        foreach (IEvent @event in events)
        {
            await RaiseAsync(@event);
        }
    }
}
