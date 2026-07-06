using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the rotational properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="InitialAngle">Initial angle of rotation, in degrees. Range: [1, 360].</param>
/// <param name="RotationRate">Duration of one full rotation on a 1-100 scale mapping 10⁹ to 10 s exponentially.</param>
public record SetRotation(
    int Id,
    [Range(1u, 360u)] uint InitialAngle,
    [Range(1u, 100u)] uint RotationRate
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
        var rotationRate = Range100.ExponentialScale(command.RotationRate, 1e9, 10.0);

        entity.AddComponent(new RotationC
        {
            CurrentAngle = Angle.FromDegrees(command.InitialAngle),
            RotationRate = RotationalSpeed.FromDegreesPerSecond(rotationRate)
        });
    }
}
