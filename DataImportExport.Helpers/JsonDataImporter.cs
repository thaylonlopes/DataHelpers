using DataImportExport.Helpers.Exceptions;
using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DataImportExport.Helpers
{
    public class JsonDataImporter : DataHandlerBase, IDataImporter
    {
        private readonly JsonConfigurationOptions _config;

        public JsonDataImporter(ILogger? logger, JsonConfigurationOptions config) : base(logger)
        {
            _config = config;
        }

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
}