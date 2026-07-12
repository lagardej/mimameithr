using Friflo.Engine.ECS;

namespace Brunnr.Cell;

/// <summary>Links a cell entity to its parent entity.</summary>
[ComponentKey("cell-parent-ref")]
public struct CellParentRefC : ILinkComponent
{
    /// <summary>The planet entity this cell belongs to.</summary>
    public Entity Parent { get; set; }

    /// <inheritdoc />
    public Entity GetIndexedValue() => Parent;
}
