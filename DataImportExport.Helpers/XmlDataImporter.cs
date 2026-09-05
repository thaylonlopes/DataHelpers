using System.Xml.Serialization;

namespace DataImportExport.Helpers;

/// <summary>
/// Importador leve e assíncrono de arquivos e streams XML para coleções de objetos C#.
/// </summary>
public class XmlDataImporter
{
    /// <summary>
    /// Importa uma coleção de dados a partir de um arquivo XML de forma assíncrona.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade a ser desserializada.</typeparam>
    /// <param name="filePath">O caminho do arquivo XML.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>A coleção de dados lida do arquivo.</returns>
    public async Task<IEnumerable<T>> ImportAsync<T>(string filePath, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Arquivo XML não encontrado: '{filePath}'", filePath);
        }

        var serializer = new XmlSerializer(typeof(List<T>));
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        return serializer.Deserialize(stream) is List<T> list ? list : Enumerable.Empty<T>();
    }

    /// <summary>
    /// Importa uma coleção de dados a partir de um Stream XML.
    /// </summary>
    /// <typeparam name="T">O tipo da entidade.</typeparam>
    /// <param name="stream">O stream XML aberto.</param>
    /// <returns>A coleção desserializada.</returns>
    public IEnumerable<T> Import<T>(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var serializer = new XmlSerializer(typeof(List<T>));
        return serializer.Deserialize(stream) is List<T> list ? list : Enumerable.Empty<T>();
    }
}

