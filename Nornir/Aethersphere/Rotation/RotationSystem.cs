using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Nornir.Aethersphere.Rotation;

/// <summary>
///     Advances <see cref="RotationC.CurrentAngle" /> by integrating <see cref="RotationC.RotationRate" />
///     over the tick delta. Wraps at 360°.
/// </summary>
public sealed class RotationSystem : QuerySystem<RotationC>
{
    protected override void OnUpdate()
    {
        var delta = Tick.deltaTime;
        Query.ForEachEntity((ref rotation, _) => rotation = ComputeRotation(rotation, delta));
    }

    private static RotationC ComputeRotation(RotationC rotation, float delta)
    {
        var deltaDegrees = rotation.RotationRate.DegreesPerSecond * delta;
        var next = (rotation.CurrentAngle.Degrees + deltaDegrees) % 360.0;
        rotation.CurrentAngle = Angle.FromDegrees(next);

        return rotation;
    }
}
