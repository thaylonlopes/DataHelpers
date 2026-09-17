using System.Buffers;
using System.IO.Compression;

namespace Caching.Helpers.Compression;

/// <summary>
/// Provedor de compressão GZip com bypass inteligente para payloads menores que 1 KB e pooling de buffers via ArrayPool.
/// </summary>
public class GZipCompressionProvider : ICacheCompression
{
    private const int CompressionThresholdBytes = 1024;
    private const int BufferChunkSize = 4096;
    private const byte BypassPrefix = 0x00;
    private const byte CompressedPrefix = 0x01;
    private const byte GZipMagicByte1 = 0x1F;
    private const byte GZipMagicByte2 = 0x8B;

    /// <inheritdoc/>
    public bool ShouldCompress(int payloadSizeBytes)
    {
        return payloadSizeBytes >= CompressionThresholdBytes;
    }

    /// <inheritdoc/>
    public byte[] Compress(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (!ShouldCompress(data.Length))
        {
            return CreateBypassedEnvelope(data);
        }

        return CompressPayload(data);
    }

    /// <inheritdoc/>
    public byte[] Decompress(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var prefix = data[0];
        if (prefix == BypassPrefix)
        {
            return ExtractBypassedPayload(data);
        }

        if (prefix == CompressedPrefix)
        {
            return DecompressPayload(data, 1);
        }

        if (IsLegacyGZip(data))
        {
            return DecompressPayload(data, 0);
        }

        return data;
    }

    private static byte[] CreateBypassedEnvelope(byte[] data)
    {
        var output = new byte[data.Length + 1];
        output[0] = BypassPrefix;
        Buffer.BlockCopy(data, 0, output, 1, data.Length);
        return output;
    }

    private static byte[] ExtractBypassedPayload(byte[] data)
    {
        var resultLength = data.Length - 1;
        var output = new byte[resultLength];
        Buffer.BlockCopy(data, 1, output, 0, resultLength);
        return output;
    }

    private static byte[] CompressPayload(byte[] data)
    {
        using var outputStream = new MemoryStream();
        outputStream.WriteByte(CompressedPrefix);

        using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            WriteWithArrayPool(gzipStream, data);
        }

        return outputStream.ToArray();
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

    private static byte[] DecompressPayload(byte[] data, int payloadOffset)
    {
        using var inputStream = new MemoryStream(data, payloadOffset, data.Length - payloadOffset);
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        var rentedBuffer = ArrayPool<byte>.Shared.Rent(BufferChunkSize);
        try
        {
            int bytesRead;
            while ((bytesRead = gzipStream.Read(rentedBuffer, 0, rentedBuffer.Length)) > 0)
            {
                outputStream.Write(rentedBuffer, 0, bytesRead);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedBuffer);
        }

        return outputStream.ToArray();
    }

    private static bool IsLegacyGZip(byte[] data)
    {
        return data.Length >= 2 && data[0] == GZipMagicByte1 && data[1] == GZipMagicByte2;
    }
}
