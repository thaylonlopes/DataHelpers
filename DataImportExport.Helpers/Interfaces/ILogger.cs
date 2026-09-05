namespace DataImportExport.Helpers.Interfaces
{
    public interface ILogger
    {
        void LogError(Exception ex, string message);
    }
}
