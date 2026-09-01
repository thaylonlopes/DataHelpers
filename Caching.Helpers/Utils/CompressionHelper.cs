using System.IO.Compression;
using System.Text;

namespace Caching.Helpers.Utils;

/// <summary>
/// Utilitário para compressão e descompressão de payloads de dados utilizando GZip.
/// </summary>
public static class CompressionHelper
{
    /// <summary>
    /// Compacta uma string UTF-8 em um array de bytes comprimido com GZip.
    /// </summary>
    /// <param name="data">A string a ser compactada.</param>
    /// <returns>Array de bytes compactado.</returns>
    public static byte[] Compress(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var bytes = Encoding.UTF8.GetBytes(data);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            gs.Write(bytes, 0, bytes.Length);
        }
        return mso.ToArray();
    }

    /// <summary>
    /// Descompacta um array de bytes GZip de volta para uma string UTF-8.
    /// </summary>
    /// <param name="data">Array de bytes compactado.</param>
    /// <returns>String UTF-8 descompactada.</returns>
    public static string Decompress(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var msi = new MemoryStream(data);
        using var mso = new MemoryStream();
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            gs.CopyTo(mso);
        }
        return Encoding.UTF8.GetString(mso.ToArray());
    }
}
