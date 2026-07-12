using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.System;
using UnitsNet;
using static Kjarni.Nornir.Geimr.Rotation.SetRotationScale;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Handles <see cref="SetRotation" /> commands against the entity store.</summary>
public class SetRotationHandler(EntityStore store) : ICommandHandler<SetRotation>
{
    /// <inheritdoc />
    public void Handle(SetRotation command)
    {
        var entity = store.GetEntityById(command.Id);
        var rotationPeriod = RotationPeriodScale.Evaluate(command.RotationPeriod);
        var revolutionsPerSecond = 1 / rotationPeriod;

        entity.AddComponent(new RotationC
        {
            CurrentAngle = Angle.FromDegrees(command.InitialAngle),
            RotationalSpeed = RotationalSpeed.FromRevolutionsPerSecond(revolutionsPerSecond)
        });

        entity.AddTags(UpdateTiering.TagFor(Duration.FromSeconds(rotationPeriod)));
    }
}
