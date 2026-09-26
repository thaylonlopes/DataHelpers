using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataImportExport.Helpers;

/// <summary>
/// Classe base para manipuladores de importação e exportação de dados com injeção de ILogger e tratamento resiliente de erros.
/// </summary>
public abstract class DataHandlerBase
{
    /// <summary>
    /// Instância de observabilidade <see cref="ILogger"/> para registro de diagnósticos.
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DataHandlerBase"/> com fallback seguro para <see cref="NullLogger.Instance"/>.
    /// </summary>
    /// <param name="logger">Instância de log corporativo opcional.</param>
    protected DataHandlerBase(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Executa uma ação assíncrona com registro de log estruturado sob qualquer falha antes do relançamento.
    /// </summary>
    /// <param name="action">A ação a ser executada.</param>
    /// <param name="operation">O nome da operação sendo executada.</param>
    /// <returns>Uma tarefa assíncrona representando a execução.</returns>
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

    /// <summary>
    /// Executa uma função assíncrona produtora de resultado com registro de log estruturado sob qualquer falha.
    /// </summary>
    /// <typeparam name="T">O tipo do resultado retornado.</typeparam>
    /// <param name="action">A função assíncrona a ser executada.</param>
    /// <param name="operation">O nome da operação sendo executada.</param>
    /// <returns>O resultado gerado pela função.</returns>
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
