namespace Brunnr.Autodoc;

/// <summary>
///     Marks a record or class as a parameter holder (simulation inputs) for autodoc generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ParametersAttribute : Attribute;

/// <summary>
///     Documents a single simulation input field on a [Parameters] holder.
/// </summary>
/// <param name="unit">Physical unit (e.g. "K", "m/s", "Pa"). Use "-" for dimensionless.</param>
/// <param name="purpose">Short description of what this parameter controls.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ParameterAttribute(string unit, string purpose) : Attribute
{
    public string Unit { get; } = unit;
    public string Purpose { get; } = purpose;
}
