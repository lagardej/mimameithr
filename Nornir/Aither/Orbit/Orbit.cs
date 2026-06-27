using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Astronomy.OrbitalMechanics;
using UnitsNet;

namespace Nornir.Aither.Orbit;

/// <summary>Keplerian orbital state of a planetary body around its parent star.</summary>
[ComponentKey("orbit")]
[Component(group: "Aither/Orbit")]
public struct OrbitC : IComponent
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

    /// <summary>Current mean anomaly.</summary>
    /// <remarks>Computed from <see cref="InitialMeanAnomaly" /> and <see cref="OrbitalPeriod" />. Wraps at 360°.</remarks>
    [State("°", "Current mean anomaly.")] public Angle MeanAnomaly;

    /// <summary>Current true anomaly.</summary>
    /// <remarks>Computed from <see cref="MeanAnomaly" /> via Kepler's equation.</remarks>
    [State("°", "Current true anomaly.")] public Angle OrbitalAngle;

    /// <summary>Current distance between the body and its parent star.</summary>
    /// <remarks>Computed from <see cref="SemiMajorAxis" />, <see cref="Eccentricity" />, and <see cref="OrbitalAngle" />.</remarks>
    [State("m", "Current distance between the body and its parent star.")]
    public Length DistanceFromStar;
}

/// <summary>
///     Computes <see cref="OrbitC.MeanAnomaly" />, <see cref="OrbitC.OrbitalAngle" />, and
///     <see cref="OrbitC.DistanceFromStar" /> from <see cref="OrbitC.InitialMeanAnomaly" /> and elapsed time.
/// </summary>
public sealed class OrbitSystem : QuerySystem<OrbitC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.time);
        Query.ForEachEntity((ref orbit, _) =>
        {
            orbit.MeanAnomaly = KeplerOrbit.CurrentMeanAnomaly(orbit.InitialMeanAnomaly, orbit.OrbitalPeriod, elapsed);
            orbit.OrbitalAngle = KeplerOrbit.MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
            orbit.DistanceFromStar =
                KeplerOrbit.RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);
        });
    }
}
