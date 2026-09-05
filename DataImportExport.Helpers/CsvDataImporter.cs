using CsvHelper;
using CsvHelper.Configuration;
using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;

namespace DataImportExport.Helpers
{
    public class CsvDataImporter : DataHandlerBase, IDataImporter
    {
        private readonly CsvConfiguration _config;
        private readonly CsvSettings _settings;

        public CsvDataImporter(ILogger logger, CsvConfiguration config, CsvSettings settings) : base(logger)
        {
            _config = config;
            _settings = settings;
        }

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
}
