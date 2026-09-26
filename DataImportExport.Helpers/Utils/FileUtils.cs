namespace DataImportExport.Helpers.Utils;

/// <summary>
/// Utilitários assíncronos para leitura e escrita de arquivos de texto em lote e streaming.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// Lê todas as linhas de um arquivo de texto de forma assíncrona retornando uma coleção em memória.
    /// </summary>
    /// <param name="filePath">O caminho físico do arquivo a ser lido.</param>
    /// <returns>Uma lista contendo todas as linhas lidas.</returns>
    public static async Task<IEnumerable<string>> ReadLinesAsync(string filePath)
    {
        var lines = new List<string>();
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            lines.Add(line);
        }
        return lines;
    }

    /// <summary>
    /// Realiza a leitura linha a linha de um arquivo de texto de forma assíncrona via streaming enumerável (<see cref="IAsyncEnumerable{T}"/>).
    /// </summary>
    /// <param name="filePath">O caminho físico do arquivo a ser consumido.</param>
    /// <returns>Um fluxo assíncrono de linhas de texto.</returns>
    public static async IAsyncEnumerable<string> StreamLinesAsync(string filePath)
    {
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            yield return line;
        }
    }

    /// <summary>
    /// Escreve uma coleção de linhas em um arquivo de texto de forma assíncrona.
    /// </summary>
    /// <param name="filePath">O caminho do arquivo de destino.</param>
    /// <param name="lines">A coleção de linhas a serem gravadas.</param>
    /// <returns>Uma tarefa assíncrona representando a conclusão da gravação.</returns>
    public static async Task WriteLinesAsync(string filePath, IEnumerable<string> lines)
    {
        using var writer = new StreamWriter(filePath);
        foreach (var line in lines)
        {
            await writer.WriteLineAsync(line ?? string.Empty);
        }
    }
}
