using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using Nornir.Aither.BodyGeometry;
using UnitsNet;

namespace Nornir.Aither.BodyPhysics;

/// <summary>Physical properties of a planetary body.</summary>
[ComponentKey("body-physics")]
[Component(group: "Aither/BodyPhysics")]
public struct BodyPhysicsC : IComponent
{
    /// <summary>Total mass of the body.</summary>
    [Setting("kg", "Total mass of the body.")]
    public Mass Mass;

    /// <summary>Age of the body.</summary>
    [Setting("s", "Age of the body.")] public Duration Age;

    /// <summary>Gravitational acceleration at the body's surface.</summary>
    /// <remarks>Computed from <see cref="Mass" /> and <see cref="BodyGeometryC.Radius" />; stored explicitly for efficiency.</remarks>
    [State("m/s²", "Gravitational acceleration at the body's surface.")]
    public Acceleration SurfaceGravity;
}
