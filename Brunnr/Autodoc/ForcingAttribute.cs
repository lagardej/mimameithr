namespace Brunnr.Autodoc;

/// <summary>
///     Marks a record as a forcing type for autodoc generation.
///     The generator will list all [Forcing]-annotated types in the same namespace
///     under the Forcings section of the component document.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ForcingAttribute : Attribute;
