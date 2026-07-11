using Friflo.Engine.ECS;
using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Nornir.Hlothyn.Lithosphere;
using System.Numerics;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>Tectonic boundary state of an R2 cell. Only cells with an actual boundary or hot spot get this component.</summary>
[ComponentKey("hlothyn-tectonics-boundary")]
public struct TectonicsBoundaryC : IComponent
{
    /// <summary>Tectonic boundary type at this cell. Determines seismic and volcanic activity regime.</summary>
    public BoundaryType BoundaryType;

    /// <summary>Time since crust formation at this cell, approximated from distance to the nearest divergent boundary.</summary>
    public Duration CrustAge;

    /// <summary>The R0 cell that seeded this cell's plate. Look up the corresponding <see cref="TectonicsPlateC" /> entity for plate-level data.</summary>
    public CellId PlateSeedCellId;

    /// <summary>Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.</summary>
    public Speed VerticalDisplacementRate;
}

/// <summary>Tectonic plate state of an R0 cell. Every R0 cell belongs to exactly one plate and gets this component.</summary>
[ComponentKey("hlothyn-tectonics-plate")]
public struct TectonicsPlateC : IComponent
{
    /// <summary>Dominant crust composition of this cell's plate. Determines isostatic base elevation and volcanic character.</summary>
    public CrustComposition CrustComposition;

    /// <summary>Thickness of the crust for this cell's plate. Drives isostatic elevation via buoyancy in the mantle.</summary>
    public Length CrustalThickness;

    /// <summary>
    ///     Rigid-body angular velocity of this cell's plate (Euler-pole rotation axis × rate).
    ///     Same value for every R0 cell belonging to the plate. Linear velocity at a point is
    ///     <c>cross(PlateAngularVelocity, positionUnitVector)</c>.
    /// </summary>
    public Vector3 PlateAngularVelocity;

    /// <summary>The R0 cell that seeded this plate. Stable identity across all R0 cells belonging to the same plate.</summary>
    public CellId PlateSeedCellId;
}
