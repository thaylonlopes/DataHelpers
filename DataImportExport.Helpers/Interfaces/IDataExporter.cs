namespace DataImportExport.Helpers.Interfaces
{
    public interface IDataExporter
    {
        Task ExportAsync<T>(string filePath, IEnumerable<T> data);
    }
}
