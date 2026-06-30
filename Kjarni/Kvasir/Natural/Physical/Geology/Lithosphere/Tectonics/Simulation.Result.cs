using Kjarni.Kvasir.Formal.Maths.Geometry.Partitioning;
using UnitsNet;

namespace Kjarni.Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;

/// <summary>Aggregated tectonic simulation output, mapping each grid cell to its tectonic state.</summary>
/// <param name="Cells">Per-cell tectonic state, keyed by cell identifier.</param>
public sealed record TectonicsResult(Dictionary<CellId, TectonicsCell> Cells);

/// <summary>
///     Tectonic state of a single grid cell.
///     Output of <see cref="TectonicsSimulation" />.
/// </summary>
public sealed record TectonicsCell
{
    /// <summary>The R0 cell that seeded this plate. Stable identity across all cells belonging to the same plate.</summary>
    public required CellId PlateId { get; init; }

    /// <summary>Dominant crust composition at this cell. Determines isostatic base elevation and volcanic character.</summary>
    public required CrustComposition CrustComposition { get; init; }

    /// <summary>Thickness of the crust at this cell. Drives isostatic elevation via buoyancy in the mantle.</summary>
    public required Length CrustalThickness { get; init; }

    /// <summary>
    ///     Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.
    ///     Driven by boundary type and plate convergence rate.
    /// </summary>
    public required Speed VerticalDisplacementRate { get; init; }

    /// <summary>Tectonic boundary type at this cell. Determines seismic and volcanic activity regime.</summary>
    public required BoundaryType BoundaryType { get; init; }
}
