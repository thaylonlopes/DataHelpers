using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DataImportExport.Helpers;

/// <summary>
/// Exportador de dados para formato JSON utilizando System.Text.Json de forma assíncrona.
/// </summary>
public class JsonDataExporter : DataHandlerBase, IDataExporter
{
    private readonly JsonConfigurationOptions _config;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="JsonDataExporter"/>.
    /// </summary>
    /// <param name="logger">Instância de log corporativo opcional.</param>
    /// <param name="config">Opções de configuração de serialização JSON.</param>
    public JsonDataExporter(ILogger? logger, JsonConfigurationOptions config) : base(logger)
    {
        _config = config;
    }

    /// <inheritdoc/>
    public async Task ExportAsync<T>(string filePath, IEnumerable<T> data)
    {
        await ExecuteWithErrorHandling(async () =>
        {
            var jsonString = JsonSerializer.Serialize(data, _config.JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, jsonString);
        }, "JSON Export");
    }
}