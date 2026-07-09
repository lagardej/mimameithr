using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Nornir.Hlothyn.Lithosphere;
using System.Numerics;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>Aggregated tectonic simulation output, mapping each grid cell to its tectonic state.</summary>
/// <param name="Cells">Per-cell tectonic state, keyed by cell identifier.</param>
internal sealed record Result(Dictionary<CellId, TectonicsCell> Cells);

/// <summary>
///     Tectonic state of a single grid cell.
///     Output of <see cref="Simulation" />.
/// </summary>
internal sealed record TectonicsCell
{
    /// <summary>Plate seed cell that identifies the tectonic plate for this cell.</summary>
    public required CellId PlateSeedCellId { get; init; }

    /// <summary>Tectonic boundary type at this cell. Determines seismic and volcanic activity regime.</summary>
    public required BoundaryType BoundaryType { get; init; }

    /// <summary>Dominant crust composition at this cell. Determines isostatic base elevation and volcanic character.</summary>
    public required CrustComposition CrustComposition { get; init; }

    /// <summary>Thickness of the crust at this cell. Drives isostatic elevation via buoyancy in the mantle.</summary>
    public required Length CrustThickness { get; init; }

    /// <summary>The R0 cell that seeded this plate. Stable identity across all cells belonging to the same plate.</summary>
    public required CellId SeedCellId { get; init; }

    /// <summary>
    ///     Rigid-body angular velocity of this cell's plate (Euler-pole rotation axis × rate).
    ///     Same value for every cell belonging to the plate. Linear velocity at a point is
    ///     <c>cross(PlateAngularVelocity, positionUnitVector)</c>.
    /// </summary>
    public required Vector3 PlateAngularVelocity { get; init; }

    /// <summary>
    ///     Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.
    ///     Driven by boundary type and plate convergence rate.
    /// </summary>
    public required Speed VerticalDisplacementRate { get; init; }
}
