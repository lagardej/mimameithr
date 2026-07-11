using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Nornir.Hlothyn.Lithosphere;
using System.Numerics;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>
///     Aggregated tectonic simulation output. Domain objects, not a per-cell table: a <see cref="Plate" /> covers
///     many R0 cells; a <see cref="Boundary" /> happens to occupy a single R2 cell, but is a boundary fact first.
/// </summary>
/// <param name="Plates">Every plate, keyed by its seed cell identity.</param>
/// <param name="Boundaries">Every R2 cell's boundary fact, keyed by the cell it occupies.</param>
internal sealed record Result(Dictionary<CellId, Plate> Plates, Dictionary<CellId, Boundary> Boundaries);

/// <summary>A tectonic plate: a contiguous set of R0 cells sharing one rigid-body motion and one crust composition.</summary>
internal sealed record Plate
{
    /// <summary>Stable identity of this plate, taken from the R0 cell that seeded it. Key into <see cref="Result.Plates" />.</summary>
    public required CellId Id { get; init; }

    /// <summary>Dominant crust composition of this plate. Determines isostatic base elevation and volcanic character.</summary>
    public required CrustComposition CrustComposition { get; init; }

    /// <summary>Thickness of this plate's crust. Drives isostatic elevation via buoyancy in the mantle.</summary>
    public required Length CrustThickness { get; init; }

    /// <summary>
    ///     Rigid-body angular velocity of this plate (Euler-pole rotation axis × rate). Linear velocity at a point
    ///     on the plate is <c>cross(AngularVelocity, positionUnitVector)</c>.
    /// </summary>
    public required Vector3 AngularVelocity { get; init; }

    /// <summary>Every R0 cell this plate covers, including <see cref="Id" />.</summary>
    public required IReadOnlyCollection<CellId> Cells { get; init; }
}

/// <summary>
///     Tectonic boundary fact occupying a single R2 cell. Output of <see cref="Simulation" />.
///     Not "the cell's boundary property" — a boundary is a domain fact about plate interaction that happens to
///     sample at R2 resolution; it isn't owned by the cell it occupies.
/// </summary>
internal sealed record Boundary
{
    /// <summary>Tectonic boundary type at this cell. Determines seismic and volcanic activity regime.</summary>
    public required BoundaryType BoundaryType { get; init; }

    /// <summary>Time since crust formation at this cell, approximated from distance to the nearest divergent boundary.</summary>
    public required Duration CrustAge { get; init; }

    /// <summary>Identity of the plate this boundary cell belongs to. Key into <see cref="Result.Plates" />.</summary>
    public required CellId PlateId { get; init; }

    /// <summary>Identity of the neighbouring plate this boundary is shared with. Key into <see cref="Result.Plates" />. Null for <see cref="BoundaryType.None" /> and <see cref="BoundaryType.HotSpot" /> cells, which belong to only one plate.</summary>
    public required CellId? OtherPlateId { get; init; }

    /// <summary>
    ///     Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.
    ///     Driven by boundary type and plate convergence rate.
    /// </summary>
    public required Speed VerticalDisplacementRate { get; init; }
}
