namespace DataImportExport.Helpers.Interfaces;

/// <summary>
/// Contrato para serviços de exportação de dados tipados para arquivos em disco.
/// </summary>
public interface IDataExporter
{
    /// <summary>
    /// Exporta uma coleção de dados para o caminho físico especificado de forma assíncrona.
    /// </summary>
    /// <typeparam name="T">O tipo dos itens da coleção a exportar.</typeparam>
    /// <param name="filePath">O caminho completo de destino do arquivo.</param>
    /// <param name="data">A coleção de objetos a serem serializados.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da exportação.</returns>
    Task ExportAsync<T>(string filePath, IEnumerable<T> data);
}
