using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Updates <see cref="RotationC.CurrentAngle" />.</summary>
public sealed class RotationSystem : QuerySystem<RotationC>
{
    /// <summary>Creates a system that updates every entity with <see cref="RotationC" />.</summary>
    public RotationSystem()
    {
    }

    /// <summary>Creates a system limited to entities tagged with <paramref name="tier" />.</summary>
    /// <param name="tier">Update-cadence tier, e.g. from <see cref="Kjarni.Brunnr.System.UpdateTiering" />.</param>
    public RotationSystem(Tags tier) => Filter.AnyTags(tier);

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.deltaTime);
        Query.ForEachEntity((ref rotation, _) =>
            rotation.CurrentAngle = CurrentAngle(rotation.CurrentAngle, rotation.RotationalSpeed, elapsed));
    }

    /// <summary>Computes the current rotation angle of a body around its axis.</summary>
    /// <param name="previousAngle">Rotation angle at the start of this update interval.</param>
    /// <param name="rotationRate">Rate at which the body completes a full rotation.</param>
    /// <param name="elapsedTime">Elapsed time since <paramref name="previousAngle" /> was recorded.</param>
    /// <returns>Current rotation angle, wrapped to [0°, 360°).</returns>
    private static Angle CurrentAngle(Angle previousAngle, RotationalSpeed rotationRate, Duration elapsedTime) =>
        Angle.FromDegrees((previousAngle.Degrees + (rotationRate.DegreesPerSecond * elapsedTime.Seconds)) % 360.0);
}
