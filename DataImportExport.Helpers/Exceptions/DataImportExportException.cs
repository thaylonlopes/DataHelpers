namespace DataImportExport.Helpers.Exceptions;

/// <summary>
/// Exceção de domínio disparada quando ocorre uma falha irrecuperável durante fluxos de importação ou exportação de dados.
/// </summary>
public class DataImportExportException : Exception
{
    /// <summary>
    /// Inicializa uma nova instância de <see cref="DataImportExportException"/> com uma mensagem descritiva de erro.
    /// </summary>
    /// <param name="message">A mensagem que descreve a falha.</param>
    public DataImportExportException(string message) : base(message) { }

    /// <summary>
    /// Inicializa uma nova instância de <see cref="DataImportExportException"/> com mensagem e causa original.
    /// </summary>
    /// <param name="message">A mensagem descritiva do erro.</param>
    /// <param name="innerException">A exceção interna que causou a falha.</param>
    public DataImportExportException(string message, Exception innerException) : base(message, innerException) { }
}
