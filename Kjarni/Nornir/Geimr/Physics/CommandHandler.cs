using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Kvasir.Foundation;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Geimr.Physics;

/// <summary>Handles <see cref="SetPhysics" /> commands against the entity store.</summary>
public class SetPhysicsHandler(EntityStore store) : ICommandHandler<SetPhysics>
{
    private static readonly PiecewiseExponentialScale s_ageScale =
        new(Range1000, [6, 10, 11, 13], [400, 700, 1000]);

    private static readonly PiecewiseExponentialScale s_massScale =
        new(Range1000, [12, 26, 29, 32], [400, 700, 1000]);

    /// <inheritdoc />
    public void Handle(SetPhysics command)
    {
        var entity = store.GetEntityById(command.Id);

        var age = s_ageScale.Evaluate(command.Age);
        var mass = Mass.FromKilograms(s_massScale.Evaluate(command.Mass));

        entity.AddComponent(new PhysicsC { Age = Duration.FromJulianYears(age), Mass = mass });
    }
}
