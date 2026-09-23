using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataImportExport.Helpers
{
    public abstract class DataHandlerBase
    {
        protected readonly ILogger _logger;

        protected DataHandlerBase(ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        protected async Task ExecuteWithErrorHandling(Func<Task> action, string operation)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during {Operation}", operation);
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
                _logger.LogError(ex, "Error during {Operation}", operation);
                throw;
            }
        }
    }
}
