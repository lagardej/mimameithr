using Kjarni.Kvasir.Foundation.Grid;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Orogeny;

/// <summary>Aggregated orogeny simulation output, mapping each orogenic cell to its state.</summary>
/// <param name="Cells">Per-cell orogenic state, keyed by cell identifier. Only convergent-belt cells are present.</param>
public sealed record Result(Dictionary<CellId, OrogenyCell> Cells);

/// <summary>
///     Orogenic state of a single grid cell.
///     Output of <see cref="Simulation" />.
/// </summary>
public sealed record OrogenyCell
{
    /// <summary>Time elapsed since this cell's orogenic belt began forming.</summary>
    public required Duration OrogenicAge { get; init; }

    /// <summary>Crustal shortening accumulated at this cell since orogenic onset.</summary>
    public required Length AccumulatedCrustalShortening { get; init; }

    /// <summary>Fraction of original relief remaining at this cell.</summary>
    public required Ratio ReliefPotential { get; init; }
}
