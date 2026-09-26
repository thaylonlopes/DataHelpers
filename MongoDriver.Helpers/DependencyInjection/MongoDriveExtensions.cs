using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDriver.Helpers;
using MongoDriver.Helpers.Context;
using MongoDriver.Helpers.Events;
using MongoDriver.Helpers.Interceptor;
using MongoDriver.Helpers.Interface;
using MongoDriver.Helpers.Interface.Context;
using MongoDriver.Helpers.Interface.Events;

namespace AuditLogger.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro de serviços e repositórios do MongoDB no contêiner de injeção de dependência.
/// </summary>
public static class MongoDriveExtensions
{
    /// <summary>
    /// Registra a instância singleton do banco MongoDB e o contexto de transações MongoContext no escopo.
    /// </summary>
    /// <param name="services">A coleção de serviços da aplicação.</param>
    /// <returns>A própria coleção de serviços configurada.</returns>
    public static IServiceCollection AddMongoContext(this IServiceCollection services)
    {
        services.AddSingleton<IMongoClientDatabase, MongoClientDatabase>()
            .AddScoped<IMongoContext, MongoContext>();
        return services;
    }

    /// <summary>
    /// Registra o despachante de eventos e o coletor de eventos de domínio.
    /// </summary>
    /// <param name="services">A coleção de serviços da aplicação.</param>
    /// <returns>A própria coleção de serviços configurada.</returns>
    public static IServiceCollection AddEventDispatcher(this IServiceCollection services)
    {
        services.AddScoped<IEventCatcher, EventCatcher>()
            .AddTransient<IRaiser, Raiser>();
        return services;
    }

    /// <summary>
    /// Registra o interceptor de eventos de domínio para interceptar chamadas ao SaveChanges.
    /// </summary>
    /// <param name="services">A coleção de serviços da aplicação.</param>
    /// <returns>A própria coleção de serviços configurada.</returns>
    public static IServiceCollection AddEventsInterceptor(this IServiceCollection services)
    {
        services.AddEventDispatcher()
            .AddTransient<SaveChangesInterceptor, CaptureEventsInterceptor>();
        return services;
    }

    /// <summary>
    /// Registra todos os componentes essenciais do MongoDriver (contexto, repositórios de comando e consulta e interceptors).
    /// </summary>
    /// <param name="services">A coleção de serviços da aplicação.</param>
    /// <param name="configuration">A instância de configuração contendo parâmetros de conexão.</param>
    /// <returns>A própria coleção de serviços configurada.</returns>
    public static IServiceCollection AddMongoDriveHelpers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoContext()
            .AddEventsInterceptor();

        services.AddScoped(typeof(ICommandRepository<>), typeof(MongoCommandRepository<>));
        services.AddScoped(typeof(IQueryRepository<>), typeof(MongoQueryRepository<>));

        return services;
    }
}
