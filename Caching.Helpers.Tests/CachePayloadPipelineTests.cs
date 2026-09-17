using System.Security.Cryptography;
using Caching.Helpers.Compression;
using Caching.Helpers.Pipeline;
using Caching.Helpers.Security;
using FluentAssertions;
using Xunit;

namespace Caching.Helpers.Tests;

public class CachePayloadPipelineTests
{
    private record ClienteSessaoDto(int Id, string Nome, string Token, decimal Saldo);

    [Fact]
    public void Pipeline_ShouldCorrectlyEncodeAndDecode_WithBothCompressionAndEncryptionEnabled()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);

        var compression = new GZipCompressionProvider();
        using var encryption = new AesEncryptionProvider(key);
        var pipeline = new CachePayloadPipeline(compression, encryption);

        var sessaoOriginal = new ClienteSessaoDto(1001, "Thaylon Lopes", "jwt-super-secret-token-xyz", 9850.50m);

        var encodedBytes = pipeline.Encode(sessaoOriginal);
        var decodedSessao = pipeline.Decode<ClienteSessaoDto>(encodedBytes);

        encodedBytes.Should().NotBeNullOrEmpty();
        decodedSessao.Should().NotBeNull();
        decodedSessao.Should().BeEquivalentTo(sessaoOriginal);
    }

    [Fact]
    public void Pipeline_ShouldWorkWithCompressionOnly()
    {
        var compression = new GZipCompressionProvider();
        var pipeline = new CachePayloadPipeline(compression: compression);

        var sessao = new ClienteSessaoDto(2002, "Maria Silva", "token-sessao-2002", 1500.00m);

        var encoded = pipeline.Encode(sessao);
        var decoded = pipeline.Decode<ClienteSessaoDto>(encoded);

        decoded.Should().BeEquivalentTo(sessao);
    }

    [Fact]
    public void Pipeline_ShouldWorkWithEncryptionOnly()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        using var encryption = new AesEncryptionProvider(key);
        var pipeline = new CachePayloadPipeline(encryption: encryption);

        var sessao = new ClienteSessaoDto(3003, "Carlos Eduardo", "token-sessao-3003", 5400.00m);

        var encoded = pipeline.Encode(sessao);
        var decoded = pipeline.Decode<ClienteSessaoDto>(encoded);

        decoded.Should().BeEquivalentTo(sessao);
    }

    [Fact]
    public void Decode_ShouldReturnDefault_WhenPayloadIsNull()
    {
        var pipeline = new CachePayloadPipeline();
        var result = pipeline.Decode<ClienteSessaoDto>(null);

        result.Should().BeNull();
    }
}
