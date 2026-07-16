using Brunnr.Cell;
using Brunnr.Grid;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Nornir.Geimr.Geometry;

/// <summary>Tag indicating that <see cref="AddGrid" /> should create R0 grid cells for a body.</summary>
public struct SeedCellGridTag : ITag;

/// <summary>
///     Creates R0 grid cell entities for a body once its <see cref="GeometryC" /> is set. Cell partition is
///     a pure function of grid shape/resolution — no designer or player parameters — so it runs
///     automatically as a system rather than through a generation command.
/// </summary>
/// <remarks>
///     Domain-agnostic: only writes <see cref="CellIdentityC" />/<see cref="CellParentRefC" />. Elements that
///     need per-cell data of their own (e.g. <c>Eldr.Irradiance</c>) provision it separately, once these
///     cells exist. Entity/component creation happens via the system's <c>CommandBuffer</c>, not directly —
///     <c>Query.ForEachEntity</c> throws <c>StructuralChangeException</c> on direct structural changes while
///     iterating.
/// </remarks>
public sealed class AddGrid : QuerySystem<GeometryC>
{
    /// <summary>Creates a new system instance.</summary>
    public AddGrid() => Filter.AnyTags(Tags.Get<SeedCellGridTag>());

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var buffer = CommandBuffer;

        Query.ForEachEntity((ref geometry, entity) =>
        {
            var grid = GridProvider.Get(geometry.GridShape);
            foreach (var cellId in grid.RootCells())
            {
                var cellEntityId = buffer.CreateEntity();
                buffer.AddComponent(cellEntityId, new CellIdentityC { Id = cellId });
                buffer.AddComponent(cellEntityId, new CellParentRefC { Parent = entity });
            }

            buffer.RemoveTag<SeedCellGridTag>(entity.Id);
        });
    }
}
