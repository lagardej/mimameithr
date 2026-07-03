using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the physical properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Age">Age of the body, in millions of years. Range: [1, 13 000].</param>
/// <param name="Mass">Mass of the body, in units of 10²¹ kg. Range: [1, 30 000 000].</param>
public record SetPhysics(
    int Id,
    [Range(1u, 13_000u)] uint Age,
    [Range(1u, 30_000_000u)] uint Mass
) : ICommand;

/// <summary>Handles <see cref="SetPhysics" /> commands against the entity store.</summary>
public class SetPhysicsHandler(EntityStore store) : ICommandHandler<SetPhysics>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetPhysics);

    /// <inheritdoc />
    public void Handle(SetPhysics command)
    {
        var entity = store.GetEntityById(command.Id);
        entity.AddComponent(new PhysicsC
        {
            Age = Duration.FromSeconds(command.Age * 1e6 * 365.25 * 24 * 3600),
            Mass = Mass.FromKilograms(command.Mass * 1e21)
        });
    }
}
