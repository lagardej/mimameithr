namespace Brunnr.Autodoc;

/// <summary>Marks a static class as a component group for autodoc generation.</summary>
/// <param name="summary">Optional short description of the group.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GroupAttribute(string summary = "") : Attribute
{
    public string Summary { get; } = summary;
}

/// <summary>
///     Marks a class as an engine component for autodoc generation.
///     The generator will produce a reference document at the component's folder root.
/// </summary>
/// <param name="group">
///     Slash-delimited group path (e.g. "Aither/BodyGeometry"). Top-level segment maps to a section in the
///     index.
/// </param>
/// <param name="title">Optional display title. Defaults to the component's folder name.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ComponentAttribute(string group = "", string title = "") : Attribute
{
    public string Group { get; } = group;
    public string Title { get; } = title;
}

/// <summary>Documents a single simulation input field on a component struct.</summary>
/// <param name="unit">Physical unit (e.g. "K", "m/s", "Pa"). Use "-" for dimensionless.</param>
/// <param name="purpose">Short description of what this parameter controls.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SettingAttribute(string unit, string purpose) : Attribute
{
    public string Unit { get; } = unit;
    public string Purpose { get; } = purpose;
}

/// <summary>Documents a single exposed state field on a component struct.</summary>
/// <param name="unit">Physical unit (e.g. "K", "m/s", "Pa"). Use "-" for dimensionless.</param>
/// <param name="purpose">Short description of what this value represents.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class StateAttribute(string unit, string purpose) : Attribute
{
    public string Unit { get; } = unit;
    public string Purpose { get; } = purpose;
}

/// <summary>
///     Marks a record as a forcing type for autodoc generation.
///     The generator will list all [Forcing]-annotated types in the same namespace
///     under the Forcings section of the component document.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ForcingAttribute : Attribute;
