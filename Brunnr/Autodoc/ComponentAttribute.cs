namespace Brunnr.Autodoc;

/// <summary>
///     Marks a class as an engine component for autodoc generation.
///     The generator will produce a reference document at the component's folder root.
/// </summary>
/// <param name="title">Optional display title. Defaults to the component's folder name.</param>
/// <param name="summary">Optional short description of the component.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ComponentAttribute(string? title = null, string? summary = null) : Attribute
{
    public string? Title { get; } = title;
    public string? Summary { get; } = summary;
}
