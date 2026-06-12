namespace Brunnr.Autodoc;

/// <summary>
///     Marks a record or class as a state holder (exposed outputs) for autodoc generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class StatesAttribute : Attribute;

/// <summary>
///     Documents a single exposed state field on a [States] holder.
/// </summary>
/// <param name="unit">Physical unit (e.g. "K", "m/s", "Pa"). Use "-" for dimensionless.</param>
/// <param name="purpose">Short description of what this value represents.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class StateAttribute(string unit, string purpose) : Attribute
{
    public string Unit { get; } = unit;
    public string Purpose { get; } = purpose;
}
