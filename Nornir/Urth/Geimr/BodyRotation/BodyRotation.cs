using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Nornir.Urth.Geimr.BodyRotation;

/// <summary>Rotational state of a planetary body.</summary>
[ComponentKey("body-rotation-settings")]
[Component("Geimr/BodyRotationSettings")]
public struct BodyRotationSettingsC : IComponent
{
    /// <summary>Rate at which the body completes a full rotation.</summary>
    [Setting("°/s", "Rate at which the body completes a full rotation.")]
    public RotationalSpeed RotationRate;
}

/// <summary>Command to configure the rotational properties of a planetary body.</summary>
/// <param name="RotationRate">Duration of one full rotation, in hours. Range: [1, 1 000].</param>
public record ConfigureBodyRotation(
    [property: Range(1u, 1_000u)] uint RotationRate
);

/// <summary>Handles <see cref="ConfigureBodyRotation" /> commands against the entity store.</summary>
public class BodyRotationHandler(EntityStore store) : ICommandHandler
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(ConfigureBodyRotation);

    /// <inheritdoc />
    public void Handle(int id, object command) => Handle(id, (ConfigureBodyRotation) command);

    /// <summary>Replaces the <see cref="BodyRotationSettingsC" /> on the given entity.</summary>
    private void Handle(int id, ConfigureBodyRotation command)
    {
        var entity = store.GetEntityById(id);
        entity.AddComponent(new BodyRotationSettingsC
        {
            RotationRate = RotationalSpeed.FromDegreesPerSecond(360.0 / (command.RotationRate * 3600.0))
        });
    }
}
