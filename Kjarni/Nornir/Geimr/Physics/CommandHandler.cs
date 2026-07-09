using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Geimr.Geometry;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Geimr.Physics;

/// <summary>Handles <see cref="SetPhysics" /> commands against the entity store.</summary>
public class SetPhysicsHandler(EntityStore store) : ICommandHandler<SetPhysics>
{
    /// <inheritdoc />
    public void Handle(SetPhysics command)
    {
        var entity = store.GetEntityById(command.Id);
        var radius = entity.GetComponent<GeometryC>().Radius;
        var age = Range100.PiecewiseExponentialScale(command.Age, [6, 10, 11, 13]);
        var mass = Mass.FromKilograms(Range100.PiecewiseExponentialScale(command.Mass, [12, 26, 29, 32]));

        entity.AddComponent(new PhysicsC
        {
            Age = Duration.FromJulianYears(age), Mass = mass, Gravity = SurfaceGravity(mass, radius)
        });
    }

    private static Acceleration SurfaceGravity(Mass mass, Length radius)
    {
        // ReSharper disable once InconsistentNaming
        // Gravitational constant
        const double G = 6.67430e-11;

        return Acceleration.FromMetersPerSecondSquared(G * mass.Kilograms / (radius.Meters * radius.Meters));
    }
}
