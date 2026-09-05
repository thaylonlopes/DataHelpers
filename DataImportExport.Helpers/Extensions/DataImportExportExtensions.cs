using System.Globalization;
using CsvHelper.Configuration;
using DataImportExport.Helpers;
using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Métodos de extensão para registro dos importadores e exportadores de dados no contêiner de DI.
/// </summary>
public static class DataImportExportExtensions
{
    /// <summary>
    /// Registra os serviços de importação e exportação de CSV, Excel e JSON no contêiner de DI.
    /// </summary>
    /// <param name="services">Coleção de serviços de injeção de dependência.</param>
    /// <returns>A coleção de serviços para encadeamento.</returns>
    public static IServiceCollection AddDataImportExport(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ILogger, SimpleLogger>();
        services.AddSingleton(new CsvSettings());
        services.AddSingleton(new JsonConfigurationOptions());
        services.AddSingleton(new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true });

        services.AddTransient<CsvDataImporter>();
        services.AddTransient<CsvDataExporter>();
        services.AddTransient<ExcelDataImporter>();
        services.AddTransient<ExcelDataExporter>();
        services.AddTransient<JsonDataImporter>();
        services.AddTransient<JsonDataExporter>();
        services.AddTransient<XmlDataImporter>();
        services.AddTransient<XmlDataExporter>();

        return services;
    }
}

