using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Rotational state of a planetary body.</summary>
[ComponentKey("geimr-rotation")]
public struct RotationC : IComponent
{
    /// <summary>Current rotation angle around the body's axis.</summary>
    public Angle CurrentAngle;

    /// <summary>Rate at which the body completes a full rotation.</summary>
    public RotationalSpeed RotationalSpeed;
}
