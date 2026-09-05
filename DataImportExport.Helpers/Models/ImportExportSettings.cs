using System.Text.Json;

namespace DataImportExport.Helpers.Models
{
    public class ImportExportSettings
    {
        public required CsvSettings CsvSettings { get; set; } = null!;
        public required JsonSerializerOptions JsonSerializerOptions { get; set; } = null!;
        public required ExcelSettings ExcelSettings { get; set; } = null!;
    }
}
