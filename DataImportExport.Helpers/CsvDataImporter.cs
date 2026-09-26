using CsvHelper;
using CsvHelper.Configuration;
using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;
using Microsoft.Extensions.Logging;

namespace DataImportExport.Helpers;

/// <summary>
/// Importador de arquivos CSV para coleções tipadas utilizando o CsvHelper com resiliência.
/// </summary>
public class CsvDataImporter : DataHandlerBase, IDataImporter
{
    private readonly CsvConfiguration _config;
    private readonly CsvSettings _settings;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="CsvDataImporter"/>.
    /// </summary>
    /// <param name="logger">Instância de log corporativo opcional.</param>
    /// <param name="config">Configuração de baixo nível do CsvHelper.</param>
    /// <param name="settings">Configurações de delimitador e cabeçalho.</param>
    public CsvDataImporter(ILogger? logger, CsvConfiguration config, CsvSettings settings) : base(logger)
    {
        _config = config;
        _settings = settings;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> ImportAsync<T>(string filePath) where T : new()
    {
        return await ExecuteWithErrorHandling(async () =>
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, _config);
            var records = new List<T>();
            await foreach (var record in csv.GetRecordsAsync<T>())
            {
                records.Add(record);
            }
            return records;
        }, "CSV Import");
    }
}
