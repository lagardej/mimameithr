using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using static Kjarni.Kvasir.Formal.Maths.Scaling;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the physical properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Age">Age of the body, on a 1-100 scale mapping 10⁶ to 10¹³ years exponentially.</param>
/// <param name="Mass">Mass of the body on a 1-100 scale mapping 10¹² kg to 10³² kg exponentially.</param>
public record SetPhysics(
    int Id,
    [Range(1u, 100u)] uint Age,
    [Range(1u, 100u)] uint Mass
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
        var age = Range100.ExponentialScale(command.Age, 1e6, 1e13);
        var mass = Range100.ExponentialScale(command.Mass, 1e12, 1e32);

        entity.AddComponent(new PhysicsC { Age = Duration.FromJulianYears(age), Mass = Mass.FromKilograms(mass) });
    }
}
