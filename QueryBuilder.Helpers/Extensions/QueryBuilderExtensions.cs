using QueryBuilder.Helpers.Core;
using QueryBuilder.Helpers.DynamoDB;
using QueryBuilder.Helpers.MongoDB;
using QueryBuilder.Helpers.MySQL;
using QueryBuilder.Helpers.Oracle;
using QueryBuilder.Helpers.PostgreSQL;
using QueryBuilder.Helpers.SQLServer;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro dos construtores de consulta no contêiner de DI.
/// </summary>
public static class QueryBuilderExtensions
{
    /// <summary>
    /// Registra os query builders poliglotas (SQL Server, PostgreSQL, MySQL, Oracle, MongoDB, DynamoDB) no contêiner de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços de injeção de dependência.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddQueryBuilders(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<SQLServerQueryBuilder>();
        services.AddTransient<PostgreSQLQueryBuilder>();
        services.AddTransient<MySQLQueryBuilder>();
        services.AddTransient<OracleQueryBuilder>();
        services.AddTransient<MongoDBQueryBuilder>();
        services.AddTransient<DynamoDBQueryBuilder>();

        return services;
    }
}

