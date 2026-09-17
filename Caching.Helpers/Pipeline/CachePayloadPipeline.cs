using System.Text.Json;
using Caching.Helpers.Compression;
using Caching.Helpers.Security;

namespace Caching.Helpers.Pipeline;

/// <summary>
/// Pipeline de processamento ordenado de payloads para caching: Serialização -> Compressão (GZip) -> Criptografia.
/// </summary>
public class CachePayloadPipeline
{
    private readonly ICacheCompression? _compression;
    private readonly ICacheEncryption? _encryption;
    private readonly JsonSerializerOptions? _jsonOptions;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CachePayloadPipeline"/>.
    /// </summary>
    /// <param name="compression">Provedor opcional de compressão (executado antes da criptografia na escrita).</param>
    /// <param name="encryption">Provedor opcional de criptografia (executado após a compressão na escrita).</param>
    /// <param name="jsonOptions">Opções opcionais de serialização JSON.</param>
    public CachePayloadPipeline(
        ICacheCompression? compression = null,
        ICacheEncryption? encryption = null,
        JsonSerializerOptions? jsonOptions = null)
    {
        _compression = compression;
        _encryption = encryption;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// Codifica um objeto aplicando o pipeline ordenado: Serialização -> Compressão (se ativa) -> Cifragem (se ativa).
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser serializado.</typeparam>
    /// <param name="value">Instância do objeto.</param>
    /// <returns>Bytes resultantes prontos para persistência no cache.</returns>
    public byte[] Encode<T>(T value)
    {
        var rawBytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);

        var processedBytes = rawBytes;
        if (_compression is not null)
        {
            processedBytes = _compression.Compress(processedBytes);
        }

        if (_encryption is not null)
        {
            processedBytes = _encryption.Encrypt(processedBytes);
        }

        return processedBytes;
    }

    /// <summary>
    /// Decodifica um payload binário aplicando o pipeline inverso: Decifragem (se ativa) -> Descompressão (se ativa) -> Deserialização.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto esperado.</typeparam>
    /// <param name="payload">Bytes obtidos do cache.</param>
    /// <returns>Instância deserializada ou default.</returns>
    public T? Decode<T>(byte[]? payload)
    {
        if (payload is null || payload.Length == 0)
        {
            return default;
        }

        var processedBytes = payload;
        if (_encryption is not null)
        {
            processedBytes = _encryption.Decrypt(processedBytes);
        }

        if (_compression is not null)
        {
            processedBytes = _compression.Decompress(processedBytes);
        }

        return JsonSerializer.Deserialize<T>(processedBytes, _jsonOptions);
    }
}
