using Brunnr.Grid;
using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Geimr.Geometry;

/// <summary>Physical geometry of a planetary body.</summary>
[ComponentKey("geimr-geometry")]
public struct GeometryC : IComponent
{
    /// <summary>Angle between the body's rotational axis and its orbital plane normal.</summary>
    public Angle AxialTilt;

    /// <summary>Grid backend shape for this body's discrete global grid.</summary>
    public GridShape GridShape;

    /// <summary>Mean radius of the body.</summary>
    public Length Radius;
}

/// <summary>Marker: this body's R0 grid cell entities have been created by <see cref="CellGridSystem" />.</summary>
[ComponentKey("geimr-cell-grid-seeded")]
public struct CellGridSeededC : IComponent
{
}
