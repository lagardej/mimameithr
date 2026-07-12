using Kjarni.Brunnr.System;
using UnitsNet;
using Xunit;

namespace Kjarni.Brunnr.Tests;

/// <summary>Coverage for the period-to-tier boundaries used to bucket entities in <c>Ver&#240;andiSystems</c>.</summary>
public class UpdateTieringTests
{
    [Theory]
    [InlineData(1)] // well below FastThreshold
    [InlineData(59)] // just below FastThreshold (60s)
    public void TagFor_ShortPeriod_ReturnsFastTier(double seconds)
    {
        var tag = UpdateTiering.TagFor(Duration.FromSeconds(seconds));

        Assert.True(tag.Has<FastTierTag>());
        Assert.False(tag.Has<MediumTierTag>());
        Assert.False(tag.Has<SlowTierTag>());
    }

    [Theory]
    [InlineData(60)] // exactly FastThreshold — boundary is inclusive on the medium side
    [InlineData(3600)] // 1 hour
    public void TagFor_MediumPeriod_ReturnsMediumTier(double seconds)
    {
        var tag = UpdateTiering.TagFor(Duration.FromSeconds(seconds));

        Assert.True(tag.Has<MediumTierTag>());
        Assert.False(tag.Has<FastTierTag>());
        Assert.False(tag.Has<SlowTierTag>());
    }

    [Fact]
    public void TagFor_LongPeriod_ReturnsSlowTier()
    {
        var tag = UpdateTiering.TagFor(Duration.FromDays(1)); // exactly SlowThreshold

        Assert.True(tag.Has<SlowTierTag>());
        Assert.False(tag.Has<FastTierTag>());
        Assert.False(tag.Has<MediumTierTag>());

        var farOut = UpdateTiering.TagFor(Duration.FromDays(365 * 30)); // 30yr rotator example
        Assert.True(farOut.Has<SlowTierTag>());
    }
}
