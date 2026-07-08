using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Handles <see cref="SetRotation" /> commands against the entity store.</summary>
public class SetRotationHandler(EntityStore store) : ICommandHandler<SetRotation>
{
    /// <inheritdoc />
    public void Handle(SetRotation command)
    {
        var entity = store.GetEntityById(command.Id);
        var rotationRate = Range100.ExponentialScale(command.RotationRate, 1e9, 10.0);

        entity.AddComponent(new RotationC
        {
            CurrentAngle = Angle.FromDegrees(command.InitialAngle),
            RotationRate = RotationalSpeed.FromDegreesPerSecond(rotationRate)
        });
    }
}
