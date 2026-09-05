using Caching.Helpers.Utils;
using FluentAssertions;
using Xunit;

namespace Caching.Helpers.Tests;

public class CompressionHelperTests
{
    [Fact]
    public void CompressAndDecompress_ShouldPreserveOriginalStringExactly()
    {
        var originalText = "Este é um texto de teste de alta entropia para validar compressão GZip com acentuação e caracteres especiais: áéíóú ç 1234567890 !@#$%^&*()";

        var compressedBytes = CompressionHelper.Compress(originalText);
        var decompressedText = CompressionHelper.Decompress(compressedBytes);

        compressedBytes.Should().NotBeNullOrEmpty();
        decompressedText.Should().Be(originalText);
    }

    [Fact]
    public void Compress_ShouldSignificantlyReduceSize_WhenTextIsRepetitiveAndLarge()
    {
        var largeText = string.Join(" | ", Enumerable.Repeat("{\"id\": 10, \"name\": \"Produto de Teste Para Alta Compressao no Redis\"}", 200));
        var rawBytesLength = System.Text.Encoding.UTF8.GetByteCount(largeText);

        var compressedBytes = CompressionHelper.Compress(largeText);

        compressedBytes.Length.Should().BeLessThan(rawBytesLength / 2);
        var restored = CompressionHelper.Decompress(compressedBytes);
        restored.Should().Be(largeText);
    }

    [Fact]
    public void Compress_ShouldThrowArgumentNullException_WhenDataIsNull()
    {
        var act = () => CompressionHelper.Compress(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Decompress_ShouldThrowArgumentNullException_WhenDataIsNull()
    {
        var act = () => CompressionHelper.Decompress(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}

