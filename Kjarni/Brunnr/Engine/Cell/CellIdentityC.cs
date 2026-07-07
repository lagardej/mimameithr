using Friflo.Engine.ECS;
using Kjarni.Kvasir.Foundation.Grid;

namespace Kjarni.Brunnr.Engine.Cell;

/// <summary>Grid cell identity. Links an entity to its position in the discrete global grid.</summary>
[ComponentKey("cell-identity")]
public struct CellIdentityC : IComponent
{
    /// <summary>Opaque cell identifier within the grid.</summary>
    public CellId Id;
}
