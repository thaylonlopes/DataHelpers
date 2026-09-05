using System.Xml.Serialization;

namespace DataImportExport.Helpers;

/// <summary>
/// Exportador leve e assíncrono de coleções de objetos para o formato XML via streaming.
/// </summary>
public class XmlDataExporter
{
    /// <summary>
    /// Exporta uma coleção de dados para um arquivo XML de forma assíncrona.
    /// </summary>
    /// <typeparam name="T">O tipo do item.</typeparam>
    /// <param name="filePath">O caminho do arquivo de destino.</param>
    /// <param name="data">A coleção de dados a ser exportada.</param>
    /// <param name="ct">Token de cancelamento.</param>
    public async Task ExportAsync<T>(string filePath, IEnumerable<T> data, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(data);

        var list = data.ToList();
        var serializer = new XmlSerializer(typeof(List<T>));

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await using var writer = new StreamWriter(stream);
        
        serializer.Serialize(writer, list);
    }

    /// <summary>
    /// Exporta uma coleção de dados para um Stream XML.
    /// </summary>
    /// <typeparam name="T">O tipo do item.</typeparam>
    /// <param name="stream">O stream de destino.</param>
    /// <param name="data">A coleção de dados.</param>
    public void Export<T>(Stream stream, IEnumerable<T> data)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(data);

        var list = data.ToList();
        var serializer = new XmlSerializer(typeof(List<T>));
        serializer.Serialize(stream, list);
    }
}

