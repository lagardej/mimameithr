using Brunnr.Data.Normalization;
using UnitsNet;
using UnitsNet.Units;
using Xunit;

namespace Brunnr.Tests.Data.Normalization;

public sealed class NormalizerTests
{
    [Fact]
    public void Normalize_RecordWithNoWrapAttributes_ReturnsSameInstance()
    {
        var record = new NoWrapRecord("test");
        var result = Normalizer.Normalize(record);
        Assert.Same(record, result);
    }

    [Fact]
    public void Normalize_ValueWithinRange_ReturnsSameInstance()
    {
        var record = new WrapRecord(Angle.FromDegrees(180));
        var result = Normalizer.Normalize(record);
        Assert.Same(record, result);
    }

    [Fact]
    public void Normalize_ValueAboveMax_WrapsAround()
    {
        var record = new WrapRecord(Angle.FromDegrees(400));
        var result = Normalizer.Normalize(record);
        Assert.Equal(40, result.Phase.Degrees, 5);
    }

    [Fact]
    public void Normalize_ValueBelowMin_WrapsAround()
    {
        var record = new WrapRecord(Angle.FromDegrees(-90));
        var result = Normalizer.Normalize(record);
        Assert.Equal(270, result.Phase.Degrees, 5);
    }

    [Fact]
    public void Normalize_NonIQuantityWithWrapAttribute_Throws()
    {
        var record = new NonQuantityRecord("not a quantity");
        Assert.Throws<InvalidOperationException>(() => Normalizer.Normalize(record));
    }

    [Fact]
    public void Normalize_ValueAboveMaxPositiveWrap_WrapsCorrectly()
    {
        var record = new WrapRecord(Angle.FromDegrees(720));
        var result = Normalizer.Normalize(record);
        Assert.Equal(0, result.Phase.Degrees, 5);
    }

    private record NoWrapRecord(string Name);

    private record WrapRecord(
        [property: WrapAround(0, 360, AngleUnit.Degree)]
        Angle Phase
    );

    private record NonQuantityRecord(
        [property: WrapAround(0, 360, AngleUnit.Degree)]
        string Phase
    );
}
