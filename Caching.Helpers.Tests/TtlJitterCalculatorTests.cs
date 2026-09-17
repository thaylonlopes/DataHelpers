using Caching.Helpers.Resilience;
using FluentAssertions;
using Xunit;

namespace Caching.Helpers.Tests;

public class TtlJitterCalculatorTests
{
    [Fact]
    public void ApplyJitter_ShouldDisperseExpirationWithinExpectedBounds()
    {
        var baseTtl = TimeSpan.FromMinutes(60);
        var minAllowed = TimeSpan.FromMinutes(60 * (1 - 0.15));
        var maxAllowed = TimeSpan.FromMinutes(60 * (1 + 0.15));

        for (var i = 0; i < 1000; i++)
        {
            var jittered = TtlJitterCalculator.ApplyJitter(baseTtl);

            jittered.Should().BeGreaterThanOrEqualTo(minAllowed);
            jittered.Should().BeLessThanOrEqualTo(maxAllowed);
        }
    }

    [Fact]
    public void ApplyJitter_ShouldGenerateDistinctValuesAcrossInvocations()
    {
        var baseTtl = TimeSpan.FromHours(1);
        var samples = Enumerable.Range(0, 100)
            .Select(_ => TtlJitterCalculator.ApplyJitter(baseTtl).TotalMilliseconds)
            .Distinct()
            .Count();

        samples.Should().BeGreaterThan(50);
    }

    [Fact]
    public void ApplyJitter_ShouldThrowArgumentOutOfRangeException_WhenBaseTtlIsZeroOrNegative()
    {
        var actZero = () => TtlJitterCalculator.ApplyJitter(TimeSpan.Zero);
        var actNegative = () => TtlJitterCalculator.ApplyJitter(TimeSpan.FromSeconds(-5));

        actZero.Should().Throw<ArgumentOutOfRangeException>();
        actNegative.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ApplyJitter_ShouldThrowArgumentException_WhenIntervalIsInvalid()
    {
        var act = () => TtlJitterCalculator.ApplyJitter(TimeSpan.FromMinutes(10), minJitterRatio: 0.20, maxJitterRatio: 0.10);

        act.Should().Throw<ArgumentException>();
    }
}
