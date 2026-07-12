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
        var tag = System.UpdateTiering.TagFor(Duration.FromSeconds(seconds));

        Assert.True(tag.Has<System.FastTierTag>());
        Assert.False(tag.Has<System.MediumTierTag>());
        Assert.False(tag.Has<System.SlowTierTag>());
    }

    [Theory]
    [InlineData(60)] // exactly FastThreshold — boundary is inclusive on the medium side
    [InlineData(3600)] // 1 hour
    public void TagFor_MediumPeriod_ReturnsMediumTier(double seconds)
    {
        var tag = System.UpdateTiering.TagFor(Duration.FromSeconds(seconds));

        Assert.True(tag.Has<System.MediumTierTag>());
        Assert.False(tag.Has<System.FastTierTag>());
        Assert.False(tag.Has<System.SlowTierTag>());
    }

    [Fact]
    public void TagFor_LongPeriod_ReturnsSlowTier()
    {
        var tag = System.UpdateTiering.TagFor(Duration.FromDays(1)); // exactly SlowThreshold

        Assert.True(tag.Has<System.SlowTierTag>());
        Assert.False(tag.Has<System.FastTierTag>());
        Assert.False(tag.Has<System.MediumTierTag>());

        var farOut = System.UpdateTiering.TagFor(Duration.FromDays(365 * 30)); // 30yr rotator example
        Assert.True(farOut.Has<System.SlowTierTag>());
    }
}
