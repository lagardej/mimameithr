using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Verthandi.Geimr.BodyGeometry;

/// <summary>Physical geometry of a planetary body.</summary>
[ComponentKey("body-geometry")]
[Component("Geimr/BodyGeometry")]
public struct BodyGeometryC : IComponent
{
    /// <summary>Mean radius of the body.</summary>
    [Setting("m", "Mean radius of the body.")]
    public Length Radius;

    /// <summary>Angle between the body's rotational axis and its orbital plane normal.</summary>
    [Setting("°", "Axial tilt relative to the orbital plane normal.")]
    public Angle AxialTilt;
}
