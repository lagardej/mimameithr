using Brunnr.Data.Validation;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using UnitsNet.Units;
using Xunit;

namespace Brunnr.Tests.Data.Validation;

public sealed class QuantityRangeAttributeTests
{
    private static ValidationResult? Validate(Angle angle, RangeBounds bounds = RangeBounds.Inclusive)
    {
        var attr = new QuantityRangeAttribute(0, 360, AngleUnit.Degree, bounds);
        var ctx = new ValidationContext(new { Phase = angle }) { MemberName = "Phase" };
        return attr.GetValidationResult(angle, ctx);
    }

    [Fact]
    public void Valid_ValueWithinRange_ReturnsSuccess()
    {
        var result = Validate(Angle.FromDegrees(180));
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void Valid_MinBoundInclusive_ReturnsSuccess()
    {
        var result = Validate(Angle.FromDegrees(0));
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void Valid_MaxBoundInclusive_ReturnsSuccess()
    {
        var result = Validate(Angle.FromDegrees(360));
        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void Invalid_BelowMin_ReturnsError()
    {
        var result = Validate(Angle.FromDegrees(-1));
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void Invalid_AboveMax_ReturnsError()
    {
        var result = Validate(Angle.FromDegrees(361));
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void Invalid_MaxBoundExclusive_ReturnsError()
    {
        var result = Validate(Angle.FromDegrees(360), RangeBounds.ExclusiveMax);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void Invalid_MinBoundExclusive_ReturnsError()
    {
        var result = Validate(Angle.FromDegrees(0), RangeBounds.ExclusiveMin);
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void Invalid_NotIQuantity_ReturnsError()
    {
        var attr = new QuantityRangeAttribute(0, 360, AngleUnit.Degree);
        var ctx = new ValidationContext(new { Phase = "not a quantity" }) { MemberName = "Phase" };
        var result = attr.GetValidationResult("not a quantity", ctx);
        Assert.NotEqual(ValidationResult.Success, result);
    }
}
