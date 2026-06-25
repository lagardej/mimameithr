using Friflo.Engine.ECS;

namespace Nornir;

/// <summary>Links a cell entity to its parent planet entity.</summary>
[ComponentKey("planet-ref")]
public struct PlanetRefC : ILinkComponent
{
    public Entity Entity { get; set; }
    public Entity GetIndexedValue() => throw new NotImplementedException();
}
