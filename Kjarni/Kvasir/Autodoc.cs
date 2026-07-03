namespace Kjarni.Kvasir;

/// <summary>
///     Marks a static class as a Kvasir science module for autodoc generation.
///     The generator will produce a reference document covering the module's physical model and public API.
/// </summary>
/// <param name="domain">
///     Slash-delimited domain path (e.g. "Natural/Physical/Geology").
///     Each segment maps to a nested section in the Kvasir index.
/// </param>
/// <param name="summary">Short description of what the module computes.</param>
[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class ModuleAttribute(string domain, string summary = "") : Attribute
{
    /// <summary>
    ///     Slash-delimited domain path (e.g. "Natural/Physical/Geology").
    ///     Each segment maps to a nested section in the Kvasir index.
    /// </summary>
    public string Domain { get; } = domain;

    /// <summary>Short description of what the module computes.</summary>
    public string Summary { get; } = summary;
}
