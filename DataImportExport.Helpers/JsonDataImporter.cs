using DataImportExport.Helpers.Exceptions;
using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DataImportExport.Helpers;

/// <summary>
/// Importador de arquivos JSON para coleções de dados fortemente tipadas via System.Text.Json.
/// </summary>
public class JsonDataImporter : DataHandlerBase, IDataImporter
{
    private readonly JsonConfigurationOptions _config;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="JsonDataImporter"/>.
    /// </summary>
    /// <param name="logger">Instância de log corporativo opcional.</param>
    /// <param name="config">Opções de configuração de desserialização JSON.</param>
    public JsonDataImporter(ILogger? logger, JsonConfigurationOptions config) : base(logger)
    {
        _config = config;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> ImportAsync<T>(string filePath) where T : new()
    {
        try
        {
            var jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<IEnumerable<T>>(jsonString) ?? Enumerable.Empty<T>();
        }
        catch (Exception ex)
        {
            throw new DataImportExportException($"Failed to import JSON data from {filePath}", ex);
        }
    }
}