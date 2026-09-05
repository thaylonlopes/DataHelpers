using DataImportExport.Helpers.Interfaces;

namespace DataImportExport.Helpers
{
    public abstract class DataHandlerBase
    {
        protected readonly ILogger _logger;

        protected DataHandlerBase(ILogger logger)
        {
            _logger = logger;
        }

        protected async Task ExecuteWithErrorHandling(Func<Task> action, string operation)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during {operation}");
                throw;
            }
        }

        protected async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> action, string operation)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during {operation}");
                throw;
            }
        }
    }
}
