using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Brunnr.Engine.Data.Validation;

/// <summary>Marks a quantity property as requiring a strictly positive value.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequirePositiveAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
    {
        if (value is not IQuantity q)
        {
            return new ValidationResult($"{ctx.MemberName} requires IQuantity.");
        }

        var v = q.As(q.Unit);
        if (double.IsNaN(v) || double.IsInfinity(v) || v <= 0)
        {
            return new ValidationResult($"{ctx.MemberName} must be > 0.");
        }

        return ValidationResult.Success;
    }
}
