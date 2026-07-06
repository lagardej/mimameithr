using Friflo.Engine.ECS.Systems;
using Kjarni.Kvasir.Geimr;
using Kjarni.Nornir.Wyrd.Geimr;
using UnitsNet;

namespace Kjarni.Nornir.Verthandi.Geimr;

/// <summary>Updates <see cref="RotationC.CurrentAngle" />.</summary>
public sealed class RotationSystem : QuerySystem<RotationC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.time);
        Query.ForEachEntity((ref rotation, _) =>
            rotation.CurrentAngle = BodyRotation.CurrentAngle(rotation.CurrentAngle, rotation.RotationRate, elapsed));
    }
}
