using System.Reflection;

namespace MongoDriver.Helpers.Models
{
    public sealed class EventBinder
    {
        public EventBinder(Type handlerType, MethodInfo handlerMethod)
        {
            HandlerType = handlerType;
            HandlerMethod = handlerMethod;
        }

        public Type HandlerType { get; }
        public MethodInfo HandlerMethod { get; }

    }
}
