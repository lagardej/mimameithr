using Kjarni.Kvasir.Formal.Maths.Geometry.Partitioning;

namespace Kjarni.Brunnr.Grid;

/// <summary>
///     Global grid instances, one per <see cref="GridShape" />. Initialized once at engine startup before any system
///     runs.
/// </summary>
public static class GridProvider
{
    private static readonly Dictionary<GridShape, IGrid> s_grids = new();

    /// <summary>Initializes the grid instance for a given shape. Must be called once per shape before the first tick.</summary>
    public static void Initialize(GridShape shape, IGrid grid) => s_grids[shape] = grid;

    /// <summary>Returns the grid instance for the given shape.</summary>
    public static IGrid Get(GridShape shape) => s_grids[shape];
}
