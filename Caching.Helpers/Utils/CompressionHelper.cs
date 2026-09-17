using System.Buffers;
using System.IO.Compression;
using System.Text;

namespace Caching.Helpers.Utils;

/// <summary>
/// Utilitário de alta performance para compressão e descompressão de payloads de dados utilizando GZip e ArrayPool.
/// </summary>
public static class CompressionHelper
{
    private const int BufferChunkSize = 4096;

    /// <summary>
    /// Compacta uma string UTF-8 em um array de bytes comprimido com GZip.
    /// </summary>
    /// <param name="data">A string a ser compactada.</param>
    /// <returns>Array de bytes compactado.</returns>
    public static byte[] Compress(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var bytes = Encoding.UTF8.GetBytes(data);
        using var outputStream = new MemoryStream();

        using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress, leaveOpen: true))
        {
            WriteWithArrayPool(gzipStream, bytes);
        }

        return outputStream.ToArray();
    }

    /// <summary>
    /// Descompacta um array de bytes GZip de volta para uma string UTF-8.
    /// </summary>
    /// <param name="data">Array de bytes compactado.</param>
    /// <returns>String UTF-8 descompactada.</returns>
    public static string Decompress(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var inputStream = new MemoryStream(data);
        using var outputStream = new MemoryStream();
        using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
        {
            ReadWithArrayPool(gzipStream, outputStream);
        }

        return Encoding.UTF8.GetString(outputStream.ToArray());
    }

    private static void WriteWithArrayPool(Stream destination, byte[] source)
    {
        var rentedBuffer = ArrayPool<byte>.Shared.Rent(BufferChunkSize);
        try
        {
            var offset = 0;
            while (offset < source.Length)
            {
                var bytesToCopy = Math.Min(BufferChunkSize, source.Length - offset);
                Buffer.BlockCopy(source, offset, rentedBuffer, 0, bytesToCopy);
                destination.Write(rentedBuffer, 0, bytesToCopy);
                offset += bytesToCopy;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedBuffer);
        }
    }

    private static void ReadWithArrayPool(Stream source, Stream destination)
    {
        var rentedBuffer = ArrayPool<byte>.Shared.Rent(BufferChunkSize);
        try
        {
            int bytesRead;
            while ((bytesRead = source.Read(rentedBuffer, 0, rentedBuffer.Length)) > 0)
            {
                destination.Write(rentedBuffer, 0, bytesRead);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedBuffer);
        }
    }
}
