using System.Text.Json;

namespace DataImportExport.Helpers.Models
{
    public class JsonConfigurationOptions
    {
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }
}
