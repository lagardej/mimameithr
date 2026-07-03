using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the rotational properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="InitialAngle">Initiangle of rotation, in degrees. Range: [1, 360].</param>
/// <param name="RotationRate">Duration of one full rotation, in hours. Range: [1, 1 000].</param>
public record SetRotation(
    int Id,
    [Range(1u, 360u)] uint InitialAngle,
    [Range(1u, 1_000u)] uint RotationRate
) : ICommand;

/// <summary>Handles <see cref="SetRotation" /> commands against the entity store.</summary>
public class SetRotationHandler(EntityStore store) : ICommandHandler<SetRotation>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetRotation);

    /// <inheritdoc />
    public void Handle(SetRotation command)
    {
        var entity = store.GetEntityById(command.Id);
        entity.AddComponent(new RotationC
        {
            CurrentAngle = Angle.FromDegrees(command.InitialAngle),
            RotationRate = RotationalSpeed.FromDegreesPerSecond(360.0 / (command.RotationRate * 3600.0))
        });
    }
}
