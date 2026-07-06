using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Kvasir.Foundation;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Handles <see cref="SetRotation" /> commands against the entity store.</summary>
public class SetRotationHandler(EntityStore store) : ICommandHandler<SetRotation>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetRotation);

    /// <inheritdoc />
    public void Handle(SetRotation command)
    {
        var entity = store.GetEntityById(command.Id);
        var rotationRate = Scaling.Range100.ExponentialScale(command.RotationRate, 1e9, 10.0);

        entity.AddComponent(new RotationC
        {
            CurrentAngle = Angle.FromDegrees(command.InitialAngle),
            RotationRate = RotationalSpeed.FromDegreesPerSecond(rotationRate)
        });
    }
}
