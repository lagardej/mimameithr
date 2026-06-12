using Brunnr.Data.Validation;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using Xunit;

namespace Brunnr.Tests.Data.Validation;

public sealed class RequirePositiveAttributeTests
{
    private static ValidationResult? Validate(object value)
    {
        var attr = new RequirePositiveAttribute();
        var ctx = new ValidationContext(new { }) { MemberName = "Value" };
        return attr.GetValidationResult(value, ctx);
    }

    [Fact]
    public void Valid_PositiveValue_ReturnsSuccess() =>
        Assert.Equal(ValidationResult.Success, Validate(Length.FromMeters(1)));

    [Fact]
    public void Invalid_ZeroValue_ReturnsError() =>
        Assert.NotEqual(ValidationResult.Success, Validate(Length.FromMeters(0)));

    [Fact]
    public void Invalid_NegativeValue_ReturnsError() =>
        Assert.NotEqual(ValidationResult.Success, Validate(Length.FromMeters(-1)));

    [Fact]
    public void Invalid_NotIQuantity_ReturnsError() =>
        Assert.NotEqual(ValidationResult.Success, Validate("not a quantity"));
}
