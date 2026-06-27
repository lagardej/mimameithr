using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Element.Aither.BodyGeometry;

/// <summary>Physical geometry of a planetary body.</summary>
[ComponentKey("body-geometry")]
public struct BodyGeometryC : IComponent
{
    /// <summary>Mean radius of the body.</summary>
    public Length Radius;

    /// <summary>Angle between the body's rotational axis and its orbital plane normal.</summary>
    public Angle AxialTilt;
}
