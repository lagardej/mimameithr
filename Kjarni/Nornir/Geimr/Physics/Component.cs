using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Physics;

/// <summary>Physical properties of a planetary body.</summary>
[ComponentKey("geimr-physics")]
public struct PhysicsC : IComponent
{
    /// <summary>Total mass of the body.</summary>
    public Mass Mass;

    /// <summary>Age of the body.</summary>
    public Duration Age;
}
