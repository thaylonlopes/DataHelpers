namespace DataImportExport.Helpers.Interfaces;

/// <summary>
/// Contrato para serviços de importação e desserialização de arquivos para coleções de dados tipadas.
/// </summary>
public interface IDataImporter
{
    /// <summary>
    /// Importa os registros de um arquivo em disco desserializando-os para o tipo de destino de forma assíncrona.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade a ser instanciada.</typeparam>
    /// <param name="filePath">O caminho do arquivo a ser lido.</param>
    /// <returns>Coleção de entidades importadas.</returns>
    Task<IEnumerable<T>> ImportAsync<T>(string filePath) where T : new();
}
