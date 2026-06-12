using Brunnr.Topology;
using System.Buffers;

namespace Brunnr.Dynacore;

/// <summary>
///     Maps <see cref="CellId" /> identifiers to stable ordinals for a given <c>(Domain, CellSet)</c> pair
///     and provides pooled scratch fields for array-based, ordinal-indexed computation.
/// </summary>
public sealed class CellIndex
{
    private readonly CellId[] _cells;
    private readonly Dictionary<CellId, int> _ordinals;

    internal CellIndex(IReadOnlyList<CellId> cells)
    {
        _cells = [.. cells];
        _ordinals = new Dictionary<CellId, int>(cells.Count);
        for (var i = 0; i < _cells.Length; i++)
        {
            _ordinals[_cells[i]] = i;
        }
    }

    /// <summary>Number of cells in this index.</summary>
    public int Count => _cells.Length;

    /// <summary>Returns the stable ordinal for <paramref name="cell" />.</summary>
    /// <exception cref="KeyNotFoundException">Cell is not in this index.</exception>
    public int Ordinal(CellId cell) => _ordinals[cell];

    /// <summary>
    ///     Returns <see langword="true" /> and sets <paramref name="ordinal" /> if <paramref name="cell" />
    ///     is present in this index; otherwise returns <see langword="false" />.
    /// </summary>
    public bool TryOrdinal(CellId cell, out int ordinal) => _ordinals.TryGetValue(cell, out ordinal);

    /// <summary>Returns the <see cref="CellId" /> at the given ordinal.</summary>
    public CellId CellAt(int ordinal) => _cells[ordinal];

    /// <summary>
    ///     Rents a scratch field of length <see cref="Count" /> from the shared array pool.
    ///     Callers must return the buffer via <see cref="ReturnField" /> after the tick.
    /// </summary>
    public double[] RentField() => ArrayPool<double>.Shared.Rent(_cells.Length);

    /// <summary>Returns a buffer previously obtained from <see cref="RentField" />.</summary>
    public static void ReturnField(double[] buffer) => ArrayPool<double>.Shared.Return(buffer, true);
}
