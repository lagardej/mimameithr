using Friflo.Engine.ECS;

namespace Nornir;

/// <summary>Links a cell entity to its parent planet entity.</summary>
[ComponentKey("planet-ref")]
public struct PlanetRefC : ILinkComponent
{
    /// <summary>The planet entity this cell belongs to.</summary>
    public Entity Entity { get; set; }

    /// <inheritdoc />
    public Entity GetIndexedValue() => Entity;
}
