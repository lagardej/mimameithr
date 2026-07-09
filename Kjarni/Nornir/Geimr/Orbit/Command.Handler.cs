using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;

namespace Kjarni.Nornir.Geimr.Orbit;

/// <summary>Handles <see cref="SetOrbit" /> commands against the entity store.</summary>
/// [Obsolete("Use Position instead")]
public class SetOrbitHandler(EntityStore store) : ICommandHandler<SetOrbit>
{
    /// <inheritdoc />
    public void Handle(SetOrbit command)
    {
        // var entity = store.GetEntityById(command.Id);
        // var orbitalPeriod = Range1000.ExponentialScale(command.OrbitalPeriod, 1e3, 1e13);
        // var semiMajorAxis = Range1000.ExponentialScale(command.SemiMajorAxis, 1e4, 1e14);
        //
        // entity.AddComponent(new OrbitC
        // {
        //     Eccentricity = command.Eccentricity / 100.0,
        //     MeanAnomaly = Angle.FromDegrees(command.InitialMeanAnomaly),
        //     OrbitalPeriod = Duration.FromSeconds(orbitalPeriod),
        //     SemiMajorAxis = Length.FromKilometers(semiMajorAxis)
        // });
        //
        // if (command.ParentId is null)
        // {
        //     return;
        // }
        //
        // var parent = store.GetEntityById(command.ParentId.Value);
        // entity.AddComponent(new OrbitParentC { Parent = parent });
    }
}
