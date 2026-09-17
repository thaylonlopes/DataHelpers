namespace Caching.Helpers.Compression;

/// <summary>
/// Contrato para compressão e descompressão de payloads de cache com suporte a bypass inteligente.
/// </summary>
public interface ICacheCompression
{
    /// <summary>
    /// Compacta os bytes informados aplicando compressão inteligente com bypass para payloads menores que o limiar.
    /// </summary>
    /// <param name="data">Bytes do payload original.</param>
    /// <returns>Bytes processados com indicador de compressão ou bypass.</returns>
    byte[] Compress(byte[] data);

    /// <summary>
    /// Descompacta os bytes processados recuperando o payload original.
    /// </summary>
    /// <param name="data">Bytes recebidos do cache.</param>
    /// <returns>Bytes descomprimidos no formato original.</returns>
    byte[] Decompress(byte[] data);

    /// <summary>
    /// Avalia se o tamanho do payload atinge o limiar mínimo de compressão (1 KB).
    /// </summary>
    /// <param name="payloadSizeBytes">Tamanho do payload em bytes.</param>
    /// <returns>Verdadeiro se o payload for maior ou igual a 1024 bytes; caso contrário, falso.</returns>
    bool ShouldCompress(int payloadSizeBytes);
}
