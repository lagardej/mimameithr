using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Astronomy.OrbitalMechanics;
using UnitsNet;

namespace Nornir.Element.Aither.Orbit;

[ComponentKey("orbit")]
public struct OrbitC : IComponent
{
    /// <summary>Semi-major axis of the orbit. Used to compute distance from star.</summary>
    public Length SemiMajorAxis;

    /// <summary>Shape of the orbital ellipse. 0 = circular, approaching 1 = parabolic limit.</summary>
    public double Eccentricity;

    /// <summary>Sidereal orbital period. Used to derive mean motion.</summary>
    public Duration OrbitalPeriod;

    /// <summary>Current mean anomaly. Integrated uniformly from mean motion each tick.</summary>
    public Angle MeanAnomaly;

    /// <summary>Current true anomaly. Derived from mean anomaly via Kepler's equation each tick.</summary>
    public Angle OrbitalAngle;

    /// <summary>
    ///     Current distance between the body and its parent star. Derived each tick from semi-major axis, eccentricity,
    ///     and true anomaly.
    /// </summary>
    public Length DistanceFromStar;
}

/// <summary>
///     Advances <see cref="OrbitC.MeanAnomaly" /> by integrating mean motion derived from
///     <see cref="OrbitC.OrbitalPeriod" />, converts to true anomaly via Kepler's equation,
///     then updates <see cref="OrbitC.OrbitalAngle" /> and <see cref="OrbitC.DistanceFromStar" />.
/// </summary>
public sealed class OrbitSystem : QuerySystem<OrbitC>
{
    protected override void OnUpdate()
    {
        var delta = Tick.deltaTime;
        Query.ForEachEntity((ref orbit, _) => orbit = ComputeOrbit(orbit, delta));
    }

    private static OrbitC ComputeOrbit(OrbitC orbit, float delta)
    {
        var meanMotionDeg = 360.0 / orbit.OrbitalPeriod.Seconds;
        orbit.MeanAnomaly = Angle.FromDegrees((orbit.MeanAnomaly.Degrees + (meanMotionDeg * delta)) % 360.0);
        orbit.OrbitalAngle = KeplerOrbit.MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
        orbit.DistanceFromStar =
            KeplerOrbit.RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);

        return orbit;
    }
}
