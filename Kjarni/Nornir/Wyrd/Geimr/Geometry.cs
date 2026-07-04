using Friflo.Engine.ECS;
using Kjarni.Brunnr.Grid;
using UnitsNet;

namespace Kjarni.Nornir.Wyrd.Geimr;

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
