using System.Security.Cryptography;
using System.Text;
using Caching.Helpers.Security;
using FluentAssertions;
using Xunit;

namespace Caching.Helpers.Tests;

public class AesEncryptionProviderTests
{
    private readonly byte[] _validKey = new byte[32];

    public AesEncryptionProviderTests()
    {
        RandomNumberGenerator.Fill(_validKey);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenKeyLengthIsNot32Bytes()
    {
        var invalidKey = new byte[16];
        var act = () => new AesEncryptionProvider(invalidKey);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*32 bytes*");
    }

    [Fact]
    public void EncryptAndDecrypt_ShouldPreserveExactOriginalData()
    {
        using var provider = new AesEncryptionProvider(_validKey);
        var plainText = "Payload confidencial de teste com dados sensíveis de cliente: CPF, Token, Cartão.";
        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        var cipherEnvelope = provider.Encrypt(plainBytes);
        var decryptedBytes = provider.Decrypt(cipherEnvelope);
        var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

        cipherEnvelope.Should().NotEqual(plainBytes);
        decryptedText.Should().Be(plainText);
    }

    [Fact]
    public void Encrypt_ShouldProduceDifferentCipherEnvelopeForSamePlaintext_DueToRandomNonce()
    {
        using var provider = new AesEncryptionProvider(_validKey);
        var plainBytes = Encoding.UTF8.GetBytes("Dado repetitivo para validar variacao de IV.");

        var envelope1 = provider.Encrypt(plainBytes);
        var envelope2 = provider.Encrypt(plainBytes);

        envelope1.Should().NotEqual(envelope2);
    }

    [Fact]
    public void Decrypt_ShouldThrowCryptographicException_WhenCiphertextIsTampered()
    {
        using var provider = new AesEncryptionProvider(_validKey);
        var plainBytes = Encoding.UTF8.GetBytes("Mensagem íntegra a ser adulterada.");

        var cipherEnvelope = provider.Encrypt(plainBytes);

        var tamperedEnvelope = (byte[])cipherEnvelope.Clone();
        tamperedEnvelope[^1] ^= 0xFF;

        var act = () => provider.Decrypt(tamperedEnvelope);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_ShouldThrowCryptographicException_WhenTagIsTampered()
    {
        using var provider = new AesEncryptionProvider(_validKey);
        var plainBytes = Encoding.UTF8.GetBytes("Mensagem com tag adulterada.");

        var cipherEnvelope = provider.Encrypt(plainBytes);

        var tamperedEnvelope = (byte[])cipherEnvelope.Clone();
        tamperedEnvelope[15] ^= 0xAA;

        var act = () => provider.Decrypt(tamperedEnvelope);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_ShouldThrowCryptographicException_WhenEnvelopeIsTooShort()
    {
        using var provider = new AesEncryptionProvider(_validKey);
        var truncated = new byte[10];

        var act = () => provider.Decrypt(truncated);

        act.Should().Throw<CryptographicException>()
            .WithMessage("*insuficiente*");
    }

    [Fact]
    public void Encrypt_ShouldReturnEmptyArray_WhenInputIsEmpty()
    {
        using var provider = new AesEncryptionProvider(_validKey);
        var result = provider.Encrypt(ReadOnlySpan<byte>.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_ShouldReturnEmptyArray_WhenInputIsEmpty()
    {
        using var provider = new AesEncryptionProvider(_validKey);
        var result = provider.Decrypt(ReadOnlySpan<byte>.Empty);

        result.Should().BeEmpty();
    }
}
