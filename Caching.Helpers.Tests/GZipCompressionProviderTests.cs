using System.Text;
using Caching.Helpers.Compression;
using FluentAssertions;
using Xunit;

namespace Caching.Helpers.Tests;

public class GZipCompressionProviderTests
{
    private readonly GZipCompressionProvider _provider = new();

    [Fact]
    public void Compress_ShouldBypassCompression_WhenPayloadIsSmallerThan1KB()
    {
        var smallPayload = Encoding.UTF8.GetBytes("Payload pequeno com menos de 1024 bytes para testar bypass.");

        var result = _provider.Compress(smallPayload);

        result.Length.Should().Be(smallPayload.Length + 1);
        result[0].Should().Be(0x00);

        var restored = _provider.Decompress(result);
        restored.Should().Equal(smallPayload);
    }

    [Fact]
    public void Compress_ShouldCompressAndReduceSizeByMoreThanSixtyPercent_WhenPayloadIsLargeAndRepetitive()
    {
        var rawJson = string.Join(",", Enumerable.Repeat("{\"id\":100,\"nome\":\"Produto Teste Alta Compressao\",\"ativo\":true}", 100));
        var largePayload = Encoding.UTF8.GetBytes(rawJson);
        largePayload.Length.Should().BeGreaterThan(1024);

        var compressed = _provider.Compress(largePayload);

        compressed[0].Should().Be(0x01);
        var reduction = 1.0 - ((double)compressed.Length / largePayload.Length);
        reduction.Should().BeGreaterThan(0.60);

        var decompressed = _provider.Decompress(compressed);
        decompressed.Should().Equal(largePayload);
    }

    [Fact]
    public void ShouldCompress_ShouldReturnExpectedBooleanBasedOnThreshold()
    {
        _provider.ShouldCompress(500).Should().BeFalse();
        _provider.ShouldCompress(1023).Should().BeFalse();
        _provider.ShouldCompress(1024).Should().BeTrue();
        _provider.ShouldCompress(5000).Should().BeTrue();
    }

    [Fact]
    public void Decompress_ShouldReturnEmptyArray_WhenInputIsEmpty()
    {
        var result = _provider.Decompress(Array.Empty<byte>());
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Concurrency_ShouldHandleConcurrentCompressionAndDecompressionSafely()
    {
        var payload = Encoding.UTF8.GetBytes(string.Join("|", Enumerable.Repeat("Concorrencia de ArrayPool e buffers", 50)));

        var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            var compressed = _provider.Compress(payload);
            var decompressed = _provider.Decompress(compressed);
            decompressed.Should().Equal(payload);
        }));

        await Task.WhenAll(tasks);
    }
}
