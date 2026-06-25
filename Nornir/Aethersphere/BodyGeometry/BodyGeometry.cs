using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Aethersphere.BodyGeometry;

[ComponentKey("body-geometry")]
public struct BodyGeometryC : IComponent
{
    /// <summary>Mean radius of the body. Used to compute surface area and solar zenith geometry.</summary>
    public Length Radius;

    /// <summary>Angle between the body's rotational axis and its orbital plane normal. Drives seasonal irradiance variation.</summary>
    public Angle AxialTilt;
}
