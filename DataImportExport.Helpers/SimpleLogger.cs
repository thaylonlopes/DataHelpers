using DataImportExport.Helpers.Interfaces;

namespace DataImportExport.Helpers
{
    public class SimpleLogger : ILogger
    {
        public void LogError(Exception ex, string message)
        {
            Console.WriteLine($"{DateTime.Now} - ERROR: {message}");
            Console.WriteLine(ex.ToString());
        }
    }
}
