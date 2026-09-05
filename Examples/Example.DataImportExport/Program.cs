using DataImportExport.Helpers;
using DataImportExport.Helpers.Models;

Console.WriteLine("=== Demo: TL.DataImportExport.Helpers ===");

var logger = new SimpleLogger();
var config = new JsonConfigurationOptions();
var exporter = new JsonDataExporter(logger, config);
var importer = new JsonDataImporter(logger, config);

var tempFile = Path.Combine(Path.GetTempPath(), "sample_export.json");
var sampleData = new List<dynamic>
{
    new { Id = 1, Name = "Alice", Role = "Developer" },
    new { Id = 2, Name = "Bob", Role = "Architect" }
};

await exporter.ExportAsync(tempFile, sampleData);
Console.WriteLine($"Exported sample data to {tempFile}");

var imported = await importer.ImportAsync<dynamic>(tempFile);
Console.WriteLine($"Imported {imported.Count()} records from JSON.");

if (File.Exists(tempFile))
    File.Delete(tempFile);
