using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using static Kjarni.Kvasir.Formal.Maths.Scaling;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the orbital parameters of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Eccentricity">Shape of the orbital ellipse, from circular to near-parabolic. Range: [1, 100].</param>
/// <param name="InitialMeanAnomaly">Position on the orbit at epoch, in degrees. Range: [1, 360].</param>
/// <param name="OrbitalPeriod">Sidereal orbital period on a 1–100 scale mapping 10³ s to 10¹³ s exponentially.</param>
/// <param name="SemiMajorAxis">Semi-major axis of the orbit on a 1–100 scale mapping 10⁴ km to 10¹⁴ km exponentially.</param>
/// <param name="ParentId">The parent id. May be null</param>
public record SetOrbit(
    int Id,
    [Range(1u, 100u)] uint Eccentricity,
    [Range(1u, 360u)] uint InitialMeanAnomaly,
    [Range(1u, 100u)] uint OrbitalPeriod,
    [Range(1u, 100u)] uint SemiMajorAxis,
    int? ParentId = null
) : ICommand;

/// <summary>Handles <see cref="SetOrbit" /> commands against the entity store.</summary>
public class SetOrbitHandler(EntityStore store) : ICommandHandler<SetOrbit>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetOrbit);

    /// <inheritdoc />
    public void Handle(SetOrbit command)
    {
        var entity = store.GetEntityById(command.Id);
        var orbitalPeriod = Range100.ExponentialScale(command.OrbitalPeriod, 1e3, 1e13);
        var semiMajorAxis = Range100.ExponentialScale(command.SemiMajorAxis, 1e4, 1e14);

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
