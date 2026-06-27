using Friflo.Engine.ECS;
using Nornir.Element.Aither.BodyGeometry;
using UnitsNet;

namespace Nornir.Element.Aither.BodyPhysics;

/// <summary>Physical properties of a planetary body.</summary>
[ComponentKey("body-physics")]
public struct BodyPhysicsC : IComponent
{
    /// <summary>Total mass of the body.</summary>
    public Mass Mass;

    /// <summary>Age of the body.</summary>
    public Duration Age;

    /// <summary>Gravitational acceleration at the body's surface.</summary>
    /// <remarks>Computed from <see cref="Mass"/> and <see cref="BodyGeometryC.Radius"/>; stored explicitly for efficiency.</remarks>
    public Acceleration SurfaceGravity;
}
