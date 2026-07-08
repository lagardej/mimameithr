using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Position;

/// <summary>Cartesian position and velocity of a planetary body.</summary>
[ComponentKey("geimr-position")]
public struct PositionC : IComponent
{
    /// <summary>Coordinates relative to the system origin.</summary>
    public Length X;

    /// <summary>Coordinates relative to the system origin.</summary>
    public Length Y;

    /// <summary>Coordinates relative to the system origin.</summary>
    public Length Z;

    /// <summary>Velocity component along X.</summary>
    public Speed VelocityX;

    /// <summary>Velocity component along Y.</summary>
    public Speed VelocityY;

    /// <summary>Velocity component along Z.</summary>
    public Speed VelocityZ;
}
