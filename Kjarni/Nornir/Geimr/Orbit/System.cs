using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Orbit;

/// <summary>
///     Updates <see cref="OrbitC.MeanAnomaly" />, <see cref="OrbitC.OrbitalAngle" />, and
///     <see cref="OrbitC.DistanceFromStar" />.
/// </summary>
public sealed class OrbitSystem : QuerySystem<OrbitC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.time);
        Query.ForEachEntity((ref orbit, _) =>
        {
            orbit.MeanAnomaly = CurrentMeanAnomaly(orbit.MeanAnomaly, orbit.OrbitalPeriod, elapsed);
            orbit.OrbitalAngle = MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
            orbit.DistanceFromStar = RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);
        });
    }

    /// <summary>
    ///     Solves Kepler's equation M = E − e·sin(E) for eccentric anomaly via Newton-Raphson,
    ///     then converts to true anomaly.
    /// </summary>
    /// <param name="meanAnomaly">Mean anomaly.</param>
    /// <param name="eccentricity">Orbital eccentricity (0 = circular, &lt;1 = elliptical).</param>
    /// <returns>True anomaly.</returns>
    private static Angle MeanToTrueAnomaly(Angle meanAnomaly, double eccentricity)
    {
        var e = meanAnomaly.Radians;
        for (var i = 0; i < 10; i++)
        {
            e -= (e - (eccentricity * Math.Sin(e)) - meanAnomaly.Radians) / (1.0 - (eccentricity * Math.Cos(e)));
        }

        return Angle.FromRadians(2.0 * Math.Atan2(
            Math.Sqrt(1.0 + eccentricity) * Math.Sin(e / 2.0),
            Math.Sqrt(1.0 - eccentricity) * Math.Cos(e / 2.0)));
    }

    /// <summary>Computes the current mean anomaly from an initial mean anomaly and elapsed time.</summary>
    /// <param name="initialMeanAnomaly">Mean anomaly at epoch (t = 0).</param>
    /// <param name="orbitalPeriod">Sidereal orbital period.</param>
    /// <param name="elapsedTime">Total elapsed time since epoch.</param>
    /// <returns>Current mean anomaly, wrapped to [0°, 360°).</returns>
    private static Angle CurrentMeanAnomaly(Angle initialMeanAnomaly, Duration orbitalPeriod, Duration elapsedTime)
    {
        var meanMotionDeg = 360.0 / orbitalPeriod.Seconds;
        return Angle.FromDegrees((initialMeanAnomaly.Degrees + (meanMotionDeg * elapsedTime.Seconds)) % 360.0);
    }

    /// <summary>
    ///     Computes the instantaneous distance from the focus (star) to the orbiting body.
    ///     Derived from the conic section equation in polar form:
    ///     <code>r = a(1 − e²) / (1 + e·cos θ)</code>
    ///     where the semi-latus rectum <c>ℓ = a(1 − e²)</c> is the orbit's half-width at the focus.
    ///     At periapsis (θ = 0°) distance is minimum; at apoapsis (θ = 180°) it is maximum.
    ///     For a circular orbit (e = 0) the result is constant and equal to <paramref name="semiMajorAxis" />.
    /// </summary>
    /// <param name="semiMajorAxis">Half the longest diameter of the ellipse.</param>
    /// <param name="eccentricity">Orbital eccentricity (0 = circular, &lt;1 = elliptical).</param>
    /// <param name="trueAnomaly">Current true anomaly (angular position from periapsis).</param>
    /// <returns>Distance from the focus to the orbiting body.</returns>
    private static Length RadiusAtTrueAnomaly(Length semiMajorAxis, double eccentricity, Angle trueAnomaly)
    {
        var semiLatusRectum = semiMajorAxis.Kilometers * (1.0 - (eccentricity * eccentricity));
        var distance = semiLatusRectum / (1.0 + (eccentricity * Math.Cos(trueAnomaly.Radians)));
        return Length.FromKilometers(distance);
    }
}
