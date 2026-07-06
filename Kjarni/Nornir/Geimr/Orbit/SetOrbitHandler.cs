using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Kvasir.Foundation;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Orbit;

/// <summary>Handles <see cref="SetOrbit" /> commands against the entity store.</summary>
public class SetOrbitHandler(EntityStore store) : ICommandHandler<SetOrbit>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetOrbit);

    /// <inheritdoc />
    public void Handle(SetOrbit command)
    {
        var entity = store.GetEntityById(command.Id);
        var orbitalPeriod = Scaling.Range100.ExponentialScale(command.OrbitalPeriod, 1e3, 1e13);
        var semiMajorAxis = Scaling.Range100.ExponentialScale(command.SemiMajorAxis, 1e4, 1e14);

        entity.AddComponent(new OrbitC
        {
            Eccentricity = command.Eccentricity / 100.0,
            MeanAnomaly = Angle.FromDegrees(command.InitialMeanAnomaly),
            OrbitalPeriod = Duration.FromSeconds(orbitalPeriod),
            SemiMajorAxis = Length.FromKilometers(semiMajorAxis)
        });

        if (command.ParentId is null)
        {
            return;
        }

        var parent = store.GetEntityById(command.ParentId.Value);
        entity.AddComponent(new OrbitParentC { Parent = parent });
    }
}
