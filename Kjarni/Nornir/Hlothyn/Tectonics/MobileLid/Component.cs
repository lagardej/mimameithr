using Friflo.Engine.ECS;
using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Nornir.Hlothyn.Lithosphere;
using System.Numerics;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>Tectonic state of a surface cell.</summary>
[ComponentKey("hlothyn-tectonics-mobilelid")]
public struct TectonicsMobileLidC : IComponent
{
    /// <summary>Plate seed cell that identifies the tectonic plate for this cell.</summary>
    public CellId PlateSeedCellId;

    /// <summary>Tectonic boundary type at this cell. Determines seismic and volcanic activity regime.</summary>
    public BoundaryType BoundaryType;

    /// <summary>Dominant crust composition at this cell. Determines isostatic base elevation and volcanic character.</summary>
    public CrustComposition CrustComposition;

    /// <summary>Thickness of the crust at this cell. Drives isostatic elevation via buoyancy in the mantle.</summary>
    public Length CrustalThickness;

    /// <summary>The R0 cell that seeded this plate. Stable identity across all cells belonging to the same plate.</summary>
    public CellId SeedCellId;

    /// <summary>
    ///     Rigid-body angular velocity of this cell's plate (Euler-pole rotation axis × rate).
    ///     Linear velocity at a point is <c>cross(PlateAngularVelocity, positionUnitVector)</c>.
    /// </summary>
    public Vector3 PlateAngularVelocity;

    /// <summary>Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.</summary>
    public Speed VerticalDisplacementRate;
}
