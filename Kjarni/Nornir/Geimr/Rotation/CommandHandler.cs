using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Kvasir.Foundation;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Handles <see cref="SetRotation" /> commands against the entity store.</summary>
public class SetRotationHandler(EntityStore store) : ICommandHandler<SetRotation>
{
    private static readonly PiecewiseExponentialScale s_rotationScale =
        new(Range1000, [9, 5, 3, 1], [400, 700, 1000]);

    /// <inheritdoc />
    public void Handle(SetRotation command)
    {
        var entity = store.GetEntityById(command.Id);
        var rotationPeriod = s_rotationScale.Evaluate(command.RotationPeriod);
        var revolutionsPerSecond = 1 / rotationPeriod;

        entity.AddComponent(new RotationC
        {
            CurrentAngle = Angle.FromDegrees(command.InitialAngle),
            RotationalSpeed = RotationalSpeed.FromRevolutionsPerSecond(revolutionsPerSecond)
        });
    }
}
