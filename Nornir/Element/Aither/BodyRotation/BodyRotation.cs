using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Nornir.Element.Aither.BodyRotation;

/// <summary>Rotational state of a planetary body.</summary>
[ComponentKey("body-rotation")]
public struct BodyRotationC : IComponent
{
    /// <summary>Rate at which the body completes a full rotation.</summary>
    public RotationalSpeed RotationRate;

    /// <summary>Current rotation angle around the body's axis.</summary>
    /// <remarks>Computed from <see cref="RotationRate"/>. Wraps at 360°.</remarks>
    public Angle CurrentAngle;
}

/// <summary>
///     Advances <see cref="BodyRotationC.CurrentAngle" /> by integrating <see cref="BodyRotationC.RotationRate" />
///     over the tick delta. Wraps at 360°.
/// </summary>
public sealed class BodyRotationSystem : QuerySystem<BodyRotationC>
{
    protected override void OnUpdate()
    {
        var delta = Tick.deltaTime;
        Query.ForEachEntity((ref rotation, _) => rotation = ComputeRotation(rotation, delta));
    }

    private static BodyRotationC ComputeRotation(BodyRotationC bodyRotation, float delta)
    {
        var deltaDegrees = bodyRotation.RotationRate.DegreesPerSecond * delta;
        var next = (bodyRotation.CurrentAngle.Degrees + deltaDegrees) % 360.0;
        bodyRotation.CurrentAngle = Angle.FromDegrees(next);

        return bodyRotation;
    }
}
