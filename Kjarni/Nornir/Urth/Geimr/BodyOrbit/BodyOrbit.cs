using Friflo.Engine.ECS;
using Kjarni.Brunnr.Autodoc;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Kjarni.Nornir.Urth.Geimr.BodyOrbit;

/// <summary>Keplerian orbital state of a planetary body around its parent star.</summary>
[ComponentKey("body-orbit-settings")]
[Component("Geimr/BodyOrbitSettings")]
public struct BodyOrbitSettingsC : IComponent
{
    /// <summary>Semi-major axis of the orbit.</summary>
    [Setting("m", "Semi-major axis of the orbit.")]
    public Length SemiMajorAxis;

    /// <summary>Shape of the orbital ellipse. 0 = circular, approaching 1 = parabolic limit.</summary>
    [Setting("-", "Shape of the orbital ellipse. 0 = circular, approaching 1 = parabolic limit.")]
    public double Eccentricity;

    /// <summary>Sidereal orbital period.</summary>
    [Setting("s", "Sidereal orbital period.")]
    public Duration OrbitalPeriod;

    /// <summary>Mean anomaly at epoch (t = 0).</summary>
    [Setting("°", "Mean anomaly at epoch (t = 0).")]
    public Angle InitialMeanAnomaly;
}

/// <summary>Command to configure the orbital parameters of a planetary body.</summary>
/// <param name="SemiMajorAxis">Semi-major axis of the orbit, in millions of km. Range: [1, 10 000].</param>
/// <param name="Eccentricity">
///     Shape of the orbital ellipse, from circular to near-parabolic. Range: [1, 100] mapped to
///     [0.01, 1.00].
/// </param>
/// <param name="OrbitalPeriod">Sidereal orbital period, in days. Range: [1, 100 000].</param>
/// <param name="InitialMeanAnomaly">Position on the orbit at epoch, in degrees. Range: [1, 360].</param>
public record ConfigureBodyOrbit(
    [property: Range(1u, 10_000u)] uint SemiMajorAxis,
    [property: Range(1u, 100u)] uint Eccentricity,
    [property: Range(1u, 100_000u)] uint OrbitalPeriod,
    [property: Range(1u, 360u)] uint InitialMeanAnomaly
);

/// <summary>Handles <see cref="ConfigureBodyOrbit" /> commands against the entity store.</summary>
public class BodyOrbitHandler(EntityStore store) : ICommandHandler
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(ConfigureBodyOrbit);

    /// <inheritdoc />
    public void Handle(int id, object command) => Handle(id, (ConfigureBodyOrbit) command);

    /// <summary>Replaces the <see cref="BodyOrbitSettingsC" /> on the given entity.</summary>
    private void Handle(int id, ConfigureBodyOrbit command)
    {
        var entity = store.GetEntityById(id);
        entity.AddComponent(new BodyOrbitSettingsC
        {
            SemiMajorAxis = Length.FromKilometers(command.SemiMajorAxis * 1e6),
            Eccentricity = command.Eccentricity / 100.0,
            OrbitalPeriod = Duration.FromSeconds(command.OrbitalPeriod * 86400.0),
            InitialMeanAnomaly = Angle.FromDegrees(command.InitialMeanAnomaly)
        });
    }
}
