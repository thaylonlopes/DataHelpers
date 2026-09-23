using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DataImportExport.Helpers
{
    public class JsonDataExporter : DataHandlerBase, IDataExporter
    {
        private readonly JsonConfigurationOptions _config;

        public JsonDataExporter(ILogger? logger, JsonConfigurationOptions config) : base(logger)
        {
            _config = config;
        }

        public async Task ExportAsync<T>(string filePath, IEnumerable<T> data)
        {
            await ExecuteWithErrorHandling(async () =>
            {
                var jsonString = JsonSerializer.Serialize(data, _config.JsonSerializerOptions);
                await File.WriteAllTextAsync(filePath, jsonString);
            }, "JSON Export");
        }
    }
}