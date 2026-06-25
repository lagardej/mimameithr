using Friflo.Engine.ECS;
using Kvasir.Science.Maths.Geometry.Partitioning;

namespace Nornir;

/// <summary>Grid cell identity. Links an entity to its position in the discrete global grid.</summary>
[ComponentKey("identity")]
public struct IdentityC : IComponent
{
    /// <summary>Opaque cell identifier within the grid.</summary>
    public CellId CellId;
}
