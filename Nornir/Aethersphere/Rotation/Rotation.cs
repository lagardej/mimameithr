using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Aethersphere.Rotation;

[ComponentKey("rotation")]
public struct RotationC : IComponent
{
    /// <summary>Rate at which the body completes a full rotation. Near-constant; set at world gen.</summary>
    public RotationalSpeed RotationRate;

    /// <summary>
    ///     Current rotation angle around the body's axis. Integrated from rotation rate each tick. Drives day/night state
    ///     per cell.
    /// </summary>
    public Angle CurrentAngle;
}
