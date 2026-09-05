using Caching.Helpers.Utils;
using FluentAssertions;
using Xunit;

namespace Caching.Helpers.Tests;

public class CacheKeyGeneratorTests
{
    [Fact]
    public void GenerateKey_ShouldReturnKeyDirectly_WhenRegionIsNull()
    {
        var key = "user:profile:100";

        var result = CacheKeyGenerator.GenerateKey(key, null);

        result.Should().Be("user:profile:100");
    }

    [Fact]
    public void GenerateKey_ShouldReturnKeyDirectly_WhenRegionIsEmpty()
    {
        var key = "order:55";

        var result = CacheKeyGenerator.GenerateKey(key, "");

        result.Should().Be("order:55");
    }

    [Fact]
    public void GenerateKey_ShouldPrefixWithRegion_WhenRegionIsProvided()
    {
        var key = "order:55";
        var region = "tenant-brazil";

        var result = CacheKeyGenerator.GenerateKey(key, region);

        result.Should().Be("tenant-brazil:order:55");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateKey_ShouldThrowArgumentException_WhenKeyIsInvalid(string? invalidKey)
    {
        var act = () => CacheKeyGenerator.GenerateKey(invalidKey!, "region");

        act.Should().Throw<ArgumentException>();
    }
}

