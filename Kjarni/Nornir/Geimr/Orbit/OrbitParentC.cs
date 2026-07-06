using Friflo.Engine.ECS;

namespace Kjarni.Nornir.Geimr.Orbit;

/// <summary>Links an orbited entity to its parent entity.</summary>
[ComponentKey("geimr-orbit-parent-ref")]
public struct OrbitParentC : ILinkComponent
{
    /// <summary>The entity this body orbits around.</summary>
    public Entity Parent;

    /// <inheritdoc />
    public Entity GetIndexedValue() => Parent;
}
