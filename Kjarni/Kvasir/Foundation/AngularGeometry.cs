using UnitsNet;

namespace Kjarni.Kvasir.Foundation;

/// <summary>Pure geometry functions for angular size and disc overlap. No domain or physical coupling — usable anywhere two circular discs or their angular sizes are involved.</summary>
public static class AngularGeometry
{
    /// <summary>Angular radius of a body as seen from a given distance.</summary>
    /// <param name="radius">Physical radius of the body.</param>
    /// <param name="distance">Distance from the observer to the body's centre.</param>
    public static Angle AngularRadius(Length radius, Length distance) =>
        Angle.FromRadians(Math.Atan(radius.Meters / distance.Meters));

    /// <summary>
    ///     Fraction of one disc obscured by another, as seen from a given observer point.
    ///     Zero when the two discs do not overlap; one for a total or annular eclipse.
    /// </summary>
    /// <param name="discAngularRadius">Angular radius of the obscured disc as seen from the observer.</param>
    /// <param name="occluderAngularRadius">Angular radius of the occluding disc as seen from the observer.</param>
    /// <param name="angularSeparation">Angular separation between the two discs' centres.</param>
    /// <returns>Obscured fraction of the disc, in [0, 1].</returns>
    public static double EclipseFraction(Angle discAngularRadius, Angle occluderAngularRadius, Angle angularSeparation)
    {
        var r1 = discAngularRadius.Radians;
        var r2 = occluderAngularRadius.Radians;
        var d = angularSeparation.Radians;

        if (d >= r1 + r2)
        {
            return 0.0;
        }

        if (d <= Math.Abs(r1 - r2))
        {
            return r2 >= r1 ? 1.0 : r2 * r2 / (r1 * r1);
        }

        var d1 = ((d * d) + (r1 * r1) - (r2 * r2)) / (2.0 * d);
        var d2 = d - d1;
        var area =
            (r1 * r1 * Math.Acos(d1 / r1)) - (d1 * Math.Sqrt((r1 * r1) - (d1 * d1))) +
            (r2 * r2 * Math.Acos(d2 / r2)) - (d2 * Math.Sqrt((r2 * r2) - (d2 * d2)));

        return area / (Math.PI * r1 * r1);
    }
}
