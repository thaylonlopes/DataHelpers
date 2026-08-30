using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDriver.Helpers;
using MongoDriver.Helpers.Context;
using MongoDriver.Helpers.Events;
using MongoDriver.Helpers.Interceptor;
using MongoDriver.Helpers.Interface;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Interface.Events;

namespace AuditLogger.DependencyInjection
{
    public static class MongoDriveExtensions
    {
        public static IServiceCollection AddMongoContext(this IServiceCollection services)
        {
            services.AddSingleton<IMongoClientDatabase,MongoClientDatabase>()
                .AddScoped<IMongoContext, MongoContext>();
            return services;
        }
        public static IServiceCollection AddEventDispatcher(this IServiceCollection services)
        {
            services.AddScoped<IEventCatcher,EventCatcher>()
                .AddTransient<IRaiser, Raiser>();
            return services;
        }
        public static IServiceCollection AddEventsInterceptor(this IServiceCollection services)
        {
            services.AddEventDispatcher()
                .AddTransient<SaveChangesInterceptor, CaptureEventsInterceptor>();
            return services;
        }

        public static IServiceCollection AddMongoDriveHelpers(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMongoContext()
                .AddEventsInterceptor();

            services.AddScoped(typeof(ICommandRepository<>), typeof(MongoCommandRepository<>));
            services.AddScoped(typeof(IQueryRepository<>), typeof(MongoQueryRepository<>));

            return services;
        }
    }
}
