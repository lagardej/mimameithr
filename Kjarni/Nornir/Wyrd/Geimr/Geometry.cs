using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Nornir.Wyrd.Geimr;

/// <summary>Physical geometry of a planetary body.</summary>
[ComponentKey("geimr-geometry")]
public struct GeometryC : IComponent
{
    /// <summary>Angle between the body's rotational axis and its orbital plane normal.</summary>
    public Angle AxialTilt;

    /// <summary>Mean radius of the body.</summary>
    public Length Radius;
}
