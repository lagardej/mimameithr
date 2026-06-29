using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Nornir.Urth.Geimr.BodyPhysics;

/// <summary>Physical properties of a planetary body.</summary>
[ComponentKey("body-physics-settings")]
[Component("Geimr/BodyPhysicsSettings")]
public struct BodyPhysicsSettingsC : IComponent
{
    /// <summary>Total mass of the body.</summary>
    [Setting("kg", "Total mass of the body.")]
    public Mass Mass;

    /// <summary>Age of the body.</summary>
    [Setting("s", "Age of the body.")] public Duration Age;
}

/// <summary>Command to configure the physical properties of a planetary body.</summary>
/// <param name="Mass">Mass of the body, in units of 10²¹ kg. Range: [1, 30 000 000].</param>
/// <param name="Age">Age of the body, in millions of years. Range: [1, 13 000].</param>
public record ConfigureBodyPhysics(
    [property: Range(1u, 30_000_000u)] uint Mass,
    [property: Range(1u, 13_000u)] uint Age
);

/// <summary>Handles <see cref="ConfigureBodyPhysics" /> commands against the entity store.</summary>
public class BodyPhysicsHandler(EntityStore store) : ICommandHandler
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(ConfigureBodyPhysics);

    /// <inheritdoc />
    public void Handle(int id, object command) => Handle(id, (ConfigureBodyPhysics) command);

    /// <summary>Replaces the <see cref="BodyPhysicsSettingsC" /> on the given entity.</summary>
    private void Handle(int id, ConfigureBodyPhysics command)
    {
        var entity = store.GetEntityById(id);
        entity.AddComponent(new BodyPhysicsSettingsC
        {
            Mass = Mass.FromKilograms(command.Mass * 1e21),
            Age = Duration.FromSeconds(command.Age * 1e6 * 365.25 * 24 * 3600)
        });
    }
}
