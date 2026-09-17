namespace Caching.Helpers.Security;

/// <summary>
/// Contrato para criptografia autenticada de payloads de cache com integridade e resistência a adulteração (Tamper Resistance).
/// </summary>
public interface ICacheEncryption
{
    /// <summary>
    /// Cifra os bytes em texto plano para proteção de dados em repouso.
    /// </summary>
    /// <param name="plainBytes">Bytes em texto plano a serem cifrados.</param>
    /// <returns>Envelope cifrado contendo versão, IV, Authentication Tag e Ciphertext.</returns>
    byte[] Encrypt(ReadOnlySpan<byte> plainBytes);

    /// <summary>
    /// Decifra os bytes do envelope validando a integridade da Authentication Tag.
    /// </summary>
    /// <param name="cipherEnvelope">Envelope cifrado contendo versão, IV, Tag e Ciphertext.</param>
    /// <returns>Bytes em texto plano recuperados com integridade comprovada.</returns>
    /// <exception cref="System.Security.Cryptography.CryptographicException">Lançada caso os dados ou a tag sofram adulteração.</exception>
    byte[] Decrypt(ReadOnlySpan<byte> cipherEnvelope);
}
