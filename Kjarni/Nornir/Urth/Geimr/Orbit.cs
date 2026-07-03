using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the orbital parameters of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="ParentId">The parent id. May be null</param>
/// <param name="Eccentricity">Shape of the orbital ellipse, from circular to near-parabolic. Range: [1, 100].</param>
/// <param name="InitialMeanAnomaly">Position on the orbit at epoch, in degrees. Range: [1, 360].</param>
/// <param name="OrbitalPeriod">Sidereal orbital period, in days. Range: [1, 100 000].</param>
/// <param name="SemiMajorAxis">Semi-major axis of the orbit, in millions of km. Range: [1, 10 000].</param>
public record SetOrbit(
    int Id,
    int? ParentId,
    [Range(1u, 100u)] uint Eccentricity,
    [Range(1u, 360u)] uint InitialMeanAnomaly,
    [Range(1u, 100_000u)] uint OrbitalPeriod,
    [Range(1u, 10_000u)] uint SemiMajorAxis
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
        // TODO : add a Relation to the ParentId
        entity.AddComponent(new OrbitC
        {
            Eccentricity = command.Eccentricity / 100.0,
            MeanAnomaly = Angle.FromDegrees(command.InitialMeanAnomaly),
            OrbitalPeriod = Duration.FromSeconds(command.OrbitalPeriod * 86400.0),
            SemiMajorAxis = Length.FromKilometers(command.SemiMajorAxis * 1e6)
        });
    }
}
