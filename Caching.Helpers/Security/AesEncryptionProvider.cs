using System.Security.Cryptography;

namespace Caching.Helpers.Security;

/// <summary>
/// Provedor de criptografia e proteção de dados em repouso com suporte a integridade e limpeza de memória (ZeroMemory).
/// </summary>
public class AesEncryptionProvider : ICacheEncryption, IDisposable
{
    private const int RequiredKeySizeBytes = 32;
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;
    private const int VersionSizeBytes = 1;
    private const int MinimumEnvelopeSizeBytes = VersionSizeBytes + NonceSizeBytes + TagSizeBytes;
    private const byte EnvelopeVersion = 0x01;

    private readonly byte[] _key;
    private bool _disposed;

    /// <summary>
    /// Inicializa o provedor criptográfico com uma chave simétrica de 256 bits (32 bytes).
    /// </summary>
    /// <param name="key">Chave de 32 bytes (256 bits) para cifragem de dados.</param>
    public AesEncryptionProvider(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.Length != RequiredKeySizeBytes)
        {
            throw new ArgumentException($"A chave deve possuir exatamente {RequiredKeySizeBytes} bytes (256 bits). Tamanho recebido: {key.Length}.", nameof(key));
        }

        _key = new byte[RequiredKeySizeBytes];
        Buffer.BlockCopy(key, 0, _key, 0, RequiredKeySizeBytes);
    }

    /// <inheritdoc/>
    public byte[] Encrypt(ReadOnlySpan<byte> plainBytes)
    {
        ThrowIfDisposed();

        if (plainBytes.IsEmpty)
        {
            return Array.Empty<byte>();
        }

        var nonce = new byte[NonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        var tag = new byte[TagSizeBytes];
        var cipherText = new byte[plainBytes.Length];

        try
        {
            using var aesGcm = new AesGcm(_key, TagSizeBytes);
            aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);

            return AssembleEnvelope(nonce, tag, cipherText);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(nonce);
            CryptographicOperations.ZeroMemory(tag);
            CryptographicOperations.ZeroMemory(cipherText);
        }
    }

    /// <inheritdoc/>
    public byte[] Decrypt(ReadOnlySpan<byte> cipherEnvelope)
    {
        ThrowIfDisposed();

        if (cipherEnvelope.IsEmpty)
        {
            return Array.Empty<byte>();
        }

        ValidateEnvelopeSize(cipherEnvelope);
        ValidateEnvelopeVersion(cipherEnvelope[0]);

        var nonce = cipherEnvelope.Slice(VersionSizeBytes, NonceSizeBytes);
        var tag = cipherEnvelope.Slice(VersionSizeBytes + NonceSizeBytes, TagSizeBytes);
        var cipherText = cipherEnvelope.Slice(MinimumEnvelopeSizeBytes);

        var plainBuffer = new byte[cipherText.Length];
        try
        {
            using var aesGcm = new AesGcm(_key, TagSizeBytes);
            aesGcm.Decrypt(nonce, cipherText, tag, plainBuffer);

            return CopyPlainResult(plainBuffer);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainBuffer);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            CryptographicOperations.ZeroMemory(_key);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    private static byte[] AssembleEnvelope(byte[] nonce, byte[] tag, byte[] cipherText)
    {
        var envelope = new byte[MinimumEnvelopeSizeBytes + cipherText.Length];
        envelope[0] = EnvelopeVersion;
        Buffer.BlockCopy(nonce, 0, envelope, VersionSizeBytes, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, envelope, VersionSizeBytes + NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(cipherText, 0, envelope, MinimumEnvelopeSizeBytes, cipherText.Length);
        return envelope;
    }

    private static void ValidateEnvelopeSize(ReadOnlySpan<byte> envelope)
    {
        if (envelope.Length < MinimumEnvelopeSizeBytes)
        {
            throw new CryptographicException("Envelope criptográfico corrompido ou com tamanho insuficiente.");
        }
    }

    private static void ValidateEnvelopeVersion(byte version)
    {
        if (version != EnvelopeVersion)
        {
            throw new CryptographicException($"Versão de envelope criptográfico desconhecida ou inválida: {version}.");
        }
    }

    private static byte[] CopyPlainResult(byte[] plainBuffer)
    {
        var result = new byte[plainBuffer.Length];
        Buffer.BlockCopy(plainBuffer, 0, result, 0, plainBuffer.Length);
        return result;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
