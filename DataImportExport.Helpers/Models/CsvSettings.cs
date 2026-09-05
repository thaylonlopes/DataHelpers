namespace DataImportExport.Helpers.Models
{
    public class CsvSettings
    {
        public bool HasHeaderRecord { get; set; } = true;
        public string Delimiter { get; set; } = ",";
    }
}
