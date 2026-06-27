namespace Kvasir.Formal.Maths.Geometry.Partitioning;

/// <summary>
///     Space partitioning contract over a discrete grid.
///     Cell identifiers are opaque <see cref="CellId" /> values.
///     All operations are deterministic and side effect free.
/// </summary>
[Module("Formal/Maths/Geometry/Partitioning", "Space partitioning contract over a discrete grid.")]
public interface IGrid
{
    #region Partition access

    /// <summary>The finest resolution level supported by this grid backend.</summary>
    Resolution MaxResolution { get; }

    /// <summary>Returns the complete top-level partition (R0 cells).</summary>
    CellId[] RootCells();

    /// <summary>Enumerates all cells at a given resolution.</summary>
    IEnumerable<CellId> CellsAtResolution(Resolution resolution);

    #endregion

    #region Cell introspection

    /// <summary>Returns the resolution of a given cell.</summary>
    Resolution ResolutionOf(CellId cell);

    /// <summary>Returns whether a cell identifier is valid.</summary>
    bool IsValid(CellId cell);

    #endregion

    #region Hierarchy traversal

    /// <summary>Returns the ancestor cell at a given coarser resolution.</summary>
    CellId ParentOf(CellId cell, Resolution parentResolution);

    /// <summary>Returns all child cells at a given finer resolution.</summary>
    CellId[] ChildrenOf(CellId cell, Resolution childResolution);

    /// <summary>
    ///     Infers the grid resolution from the first cell in a non-empty dictionary.
    ///     Returns <paramref name="fallback" /> when the dictionary is empty.
    /// </summary>
    Resolution InferResolution<T>(Dictionary<CellId, T> cells, Resolution fallback);

    #endregion

    #region Neighbourhood traversal

    /// <summary>Returns all cells within ring radius k of a centre cell, including the centre.</summary>
    CellId[] Disk(CellId cell, int k);

    /// <summary>
    ///     Enumerates cells at exactly grid-ring distance k from a centre cell.
    ///     k = 0 yields only the centre cell itself.
    /// </summary>
    IEnumerable<CellId> GridRing(CellId center, int k);

    #endregion
}
