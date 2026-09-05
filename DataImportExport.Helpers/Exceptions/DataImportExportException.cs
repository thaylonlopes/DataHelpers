namespace DataImportExport.Helpers.Exceptions
{
    public class DataImportExportException : Exception
    {
        public DataImportExportException(string message) : base(message) { }
        public DataImportExportException(string message, Exception innerException) : base(message, innerException) { }
    }
}
