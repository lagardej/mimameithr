using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Brunnr.Data.Validation;

/// <summary>
///     Validates that an <see cref="IQuantity" /> property falls within a specified range.
///     The value is extracted in the given unit before comparison.
/// </summary>
/// <example>
///     <code>
///         [QuantityRange(0, 180, AngleUnit.Degree)]
///         public required Angle Obliquity { get; init; }
///
///         [QuantityRange(0, 1, RatioUnit.DecimalFraction, RangeBounds.ExclusiveMax)]
///         public required Ratio OrbitalEccentricity { get; init; }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class QuantityRangeAttribute(
    double min,
    double max,
    object unit,
    RangeBounds bounds = RangeBounds.Inclusive)
    : ValidationAttribute
{
    private readonly Enum _unit = (Enum) unit;

    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is not IQuantity q)
        {
            return new ValidationResult($"{ctx.MemberName} requires IQuantity.");
        }

        var v = q.As(_unit);

        var minOk = bounds is RangeBounds.ExclusiveMin or RangeBounds.Exclusive ? v > min : v >= min;
        var maxOk = bounds is RangeBounds.ExclusiveMax or RangeBounds.Exclusive ? v < max : v <= max;

        if (minOk && maxOk)
        {
            return ValidationResult.Success;
        }

        var minBracket = bounds is RangeBounds.ExclusiveMin or RangeBounds.Exclusive ? "(" : "[";
        var maxBracket = bounds is RangeBounds.ExclusiveMax or RangeBounds.Exclusive ? ")" : "]";
        return new ValidationResult(
            $"{ctx.MemberName} must be in {minBracket}{min}, {max}{maxBracket} {_unit}.");
    }
}
