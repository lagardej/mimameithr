using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Geimr.Physics;

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
