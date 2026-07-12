using Brunnr.Command;
using Friflo.Engine.ECS;
using UnitsNet;
using static Nornir.Geimr.Physics.SetPhysicsScale;

namespace Nornir.Geimr.Physics;

/// <summary>Handles <see cref="SetPhysics" /> commands against the entity store.</summary>
public class SetPhysicsHandler(EntityStore store) : ICommandHandler<SetPhysics>
{
    /// <inheritdoc />
    public void Handle(SetPhysics command)
    {
        var entity = store.GetEntityById(command.Id);

        var age = AgeScale.Evaluate(command.Age);
        var mass = Mass.FromKilograms(MassScale.Evaluate(command.Mass));

        entity.AddComponent(new PhysicsC { Age = Duration.FromJulianYears(age), Mass = mass });
    }
}
