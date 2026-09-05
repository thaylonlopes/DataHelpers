using CsvHelper;
using CsvHelper.Configuration;
using DataImportExport.Helpers.Exceptions;
using DataImportExport.Helpers.Interfaces;
using DataImportExport.Helpers.Models;
using System.Globalization;

namespace DataImportExport.Helpers
{
    public class CsvDataExporter : IDataExporter
    {
        private readonly CsvConfiguration _config;
        private readonly CsvSettings _settings;

        public CsvDataExporter(CsvSettings settings)
        {
            _settings = settings;
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = _settings.HasHeaderRecord,
                Delimiter = _settings.Delimiter,
            };
        }

        public async Task ExportAsync<T>(string filePath, IEnumerable<T> data)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, _config);
                await csv.WriteRecordsAsync(data);
            }
            catch (Exception ex)
            {
                throw new DataImportExportException($"Failed to export CSV data to {filePath}", ex);
            }
        }
    }
}
