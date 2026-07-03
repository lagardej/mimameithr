using Friflo.Engine.ECS;
using Kjarni.Kvasir.Natural.Physical.Geology.Lithosphere;
using Kjarni.Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;
using UnitsNet;

namespace Kjarni.Nornir.Wyrd.Hlothyn;

/// <summary>Tectonic state of a surface cell.</summary>
[ComponentKey("hlothyn-tectonics")]
public struct TectonicsC : IComponent
{
    /// <summary> Number of tectonic plates seeded at world gen.</summary>
    public uint PlateCount;

    /// <summary> Plate size distribution.</summary>
    public uint PlateFragmentation;

    /// <summary> Rate at which plates reorganize over time. </summary>
    public uint PlateStability;

    /// <summary> Degree to which activity concentrates along plate boundaries.</summary>
    public uint BoundaryFocus;

    /// <summary> Bias toward convergent boundaries.</summary>
    public uint CollisionDominance;

    /// <summary> Number and intensity of intraplate volcanic plumes.</summary>
    public uint HotSpotDensity;

    /// <summary>Dominant crust composition at this cell.</summary>
    public CrustComposition CrustComposition;

    /// <summary>Thickness of the crust at this cell.</summary>
    public Length CrustalThickness;

    /// <summary>Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.</summary>
    public Speed VerticalDisplacementRate;

    /// <summary>Tectonic boundary type at this cell.</summary>
    public BoundaryType BoundaryType;
}
