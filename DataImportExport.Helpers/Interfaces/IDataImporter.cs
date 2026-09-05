namespace DataImportExport.Helpers.Interfaces
{
    public interface IDataImporter
    {
        Task<IEnumerable<T>> ImportAsync<T>(string filePath) where T : new();
    }
}
