using Friflo.Engine.ECS;

namespace Kjarni.Brunnr.Engine;

/// <summary>
///     Owns the shared <see cref="EntityStore" /> backing every sub-engine attached to it. Sub-engines
///     (domain, rendering, ...) are constructed with this store injected, so they operate on the same
///     entities without needing to know about each other. Brunnr never references those sub-engine types
///     — composition happens one layer up, at whichever engine is the application's entry point.
/// </summary>
public sealed class BrunnrEngine
{
    /// <summary>The shared entity store. Inject into sub-engine constructors.</summary>
    public EntityStore Store { get; } = new();
}
