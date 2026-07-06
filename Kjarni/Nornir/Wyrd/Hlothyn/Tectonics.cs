using Friflo.Engine.ECS;
using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Hlothyn.Lithosphere;
using Kjarni.Kvasir.Hlothyn.Tectonics;
using UnitsNet;

namespace Kjarni.Nornir.Wyrd.Hlothyn;

/// <summary>Tectonic state of a surface cell.</summary>
[ComponentKey("hlothyn-tectonics")]
public struct TectonicsC : IComponent
{
    /// <summary>Tectonic boundary type at this cell. Determines seismic and volcanic activity regime.</summary>
    public BoundaryType BoundaryType;

    /// <summary>Dominant crust composition at this cell. Determines isostatic base elevation and volcanic character.</summary>
    public CrustComposition CrustComposition;

    /// <summary>Thickness of the crust at this cell. Drives isostatic elevation via buoyancy in the mantle.</summary>
    public Length CrustalThickness;

    /// <summary>The R0 cell that seeded this plate. Stable identity across all cells belonging to the same plate.</summary>
    public CellId SeedCellId;

    /// <summary>Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.</summary>
    public Speed VerticalDisplacementRate;
}
