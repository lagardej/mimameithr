using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Element.Aither.BodyPhysics;

[ComponentKey("body-physics")]
public struct BodyPhysicsC : IComponent
{
    /// <summary>Total mass of the body. Drives surface gravity and mantle heat flux.</summary>
    public Mass Mass;

    /// <summary>Age of the body. Drives radiogenic heat decay and mantle heat flux.</summary>
    public Duration Age;

    /// <summary>
    ///     Gravitational acceleration at the body's surface. Derivable from mass and radius; stored explicitly for
    ///     efficiency.
    /// </summary>
    public Acceleration SurfaceGravity;
}
