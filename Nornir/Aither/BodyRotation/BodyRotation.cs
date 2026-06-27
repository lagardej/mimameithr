using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Nornir.Aither.BodyRotation;

/// <summary>Rotational state of a planetary body.</summary>
[ComponentKey("body-rotation")]
[Component(group: "Aither/BodyRotation")]
public struct BodyRotationC : IComponent
{
    /// <summary>Rate at which the body completes a full rotation.</summary>
    [Setting("°/s", "Rate at which the body completes a full rotation.")]
    public RotationalSpeed RotationRate;

    /// <summary>Rotation angle at epoch (t = 0).</summary>
    [Setting("°", "Rotation angle at epoch (t = 0).")]
    public Angle EpochAngle;

    /// <summary>Current rotation angle around the body's axis.</summary>
    /// <remarks>Computed from <see cref="EpochAngle" /> and <see cref="RotationRate" />. Wraps at 360°.</remarks>
    [State("°", "Current rotation angle around the body's axis.")]
    public Angle CurrentAngle;
}

/// <summary>
///     Advances <see cref="BodyRotationC.CurrentAngle" /> from <see cref="BodyRotationC.EpochAngle" />
///     and <see cref="BodyRotationC.RotationRate" /> over total elapsed time.
/// </summary>
public sealed class BodyRotationSystem : QuerySystem<BodyRotationC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.time);
        Query.ForEachEntity((ref rotation, _) =>
            rotation.CurrentAngle = Kvasir.Natural.Physical.Astronomy.BodyRotation.CurrentAngle(
                rotation.EpochAngle, rotation.RotationRate, elapsed));
    }
}
