using System.Data;
using Dapper.Helpers;
using Dapper.Helpers.Context;
using Dapper.Helpers.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro dos serviços do Dapper.Helpers no contêiner de DI.
/// </summary>
public static class DapperExtensions
{
    /// <summary>
    /// Registra o contexto e os serviços do Dapper utilizando a string de conexão informada.
    /// </summary>
    /// <param name="services">Coleção de serviços de injeção de dependência.</param>
    /// <param name="connectionString">String de conexão com o banco de dados.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddDapperHelpers(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));
        services.AddScoped<IUnitOfWork, DapperUnitOfWork>();
        services.AddScoped<IDapperHelper, DapperHelper>();

        return services;
    }

    /// <summary>
    /// Registra o contexto e os serviços do Dapper recuperando a string de conexão da configuração.
    /// </summary>
    /// <param name="services">Coleção de serviços de injeção de dependência.</param>
    /// <param name="configuration">Instância de configuração da aplicação.</param>
    /// <param name="connectionName">Nome da connection string no appsettings.json (padrão: "DefaultConnection").</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddDapperHelpers(this IServiceCollection services, IConfiguration configuration, string connectionName = "DefaultConnection")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found in configuration.");

        return services.AddDapperHelpers(connectionString);
    }
}

