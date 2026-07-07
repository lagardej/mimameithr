using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Updates <see cref="RotationC.CurrentAngle" />.</summary>
public sealed class RotationSystem : QuerySystem<RotationC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.time);
        Query.ForEachEntity((ref rotation, _) =>
            rotation.CurrentAngle = CurrentAngle(rotation.CurrentAngle, rotation.RotationRate, elapsed));
    }

    /// <summary>Computes the current rotation angle of a body around its axis.</summary>
    /// <param name="epochAngle">Rotation angle at epoch (t = 0).</param>
    /// <param name="rotationRate">Rate at which the body completes a full rotation.</param>
    /// <param name="elapsedTime">Total elapsed time since epoch.</param>
    /// <returns>Current rotation angle, wrapped to [0°, 360°).</returns>
    private static Angle CurrentAngle(Angle epochAngle, RotationalSpeed rotationRate, Duration elapsedTime) =>
        Angle.FromDegrees((epochAngle.Degrees + (rotationRate.DegreesPerSecond * elapsedTime.Seconds)) % 360.0);
}
