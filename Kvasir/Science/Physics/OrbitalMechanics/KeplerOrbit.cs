using UnitsNet;

namespace Kvasir.Science.Physics.OrbitalMechanics;

/// <summary>Kepler orbit equations for elliptical orbits.</summary>
public static class KeplerOrbit
{
    /// <summary>
    ///     Converts true anomaly to mean anomaly via eccentric anomaly.
    /// </summary>
    /// <param name="trueAnomaly">True anomaly.</param>
    /// <param name="eccentricity">Orbital eccentricity (0 = circular, &lt;1 = elliptical).</param>
    /// <returns>Mean anomaly.</returns>
    public static Angle TrueToMeanAnomaly(Angle trueAnomaly, double eccentricity)
    {
        var eccentricAnomaly = 2.0 * Math.Atan2(
            Math.Sqrt(1.0 - eccentricity) * Math.Sin(trueAnomaly.Radians / 2.0),
            Math.Sqrt(1.0 + eccentricity) * Math.Cos(trueAnomaly.Radians / 2.0));
        return Angle.FromRadians(eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly));
    }

    /// <summary>
    ///     Solves Kepler's equation M = E − e·sin(E) for eccentric anomaly via Newton-Raphson,
    ///     then converts to true anomaly.
    /// </summary>
    /// <param name="meanAnomaly">Mean anomaly.</param>
    /// <param name="eccentricity">Orbital eccentricity (0 = circular, &lt;1 = elliptical).</param>
    /// <returns>True anomaly.</returns>
    public static Angle MeanToTrueAnomaly(Angle meanAnomaly, double eccentricity)
    {
        var e = meanAnomaly.Radians;
        for (var i = 0; i < 10; i++)
        {
            e -= (e - eccentricity * Math.Sin(e) - meanAnomaly.Radians) / (1.0 - eccentricity * Math.Cos(e));
        }

        return Angle.FromRadians(2.0 * Math.Atan2(
            Math.Sqrt(1.0 + eccentricity) * Math.Sin(e / 2.0),
            Math.Sqrt(1.0 - eccentricity) * Math.Cos(e / 2.0)));
    }
}
