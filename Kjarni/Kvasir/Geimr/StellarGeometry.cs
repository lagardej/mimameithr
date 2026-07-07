using UnitsNet;

namespace Kjarni.Kvasir.Geimr;

/// <summary>Stellar geometry functions for computing a star's position, flux, and eclipses at a surface cell.</summary>
public static class StellarGeometry
{
    /// <summary>Angular radius of a body as seen from a given distance.</summary>
    /// <param name="radius">Physical radius of the body.</param>
    /// <param name="distance">Distance from the observer to the body's centre.</param>
    public static Angle AngularRadius(Length radius, Length distance) =>
        Angle.FromRadians(Math.Atan(radius.Meters / distance.Meters));

    /// <summary>
    ///     Fraction of a star's disc obscured by an occluding body, as seen from a given observer point.
    ///     Zero when the two discs do not overlap; one for a total or annular eclipse.
    /// </summary>
    /// <param name="starAngularRadius">Angular radius of the star as seen from the observer.</param>
    /// <param name="occluderAngularRadius">Angular radius of the occluding body as seen from the observer.</param>
    /// <param name="angularSeparation">Angular separation between the star's and occluder's centres.</param>
    /// <returns>Obscured fraction of the star's disc, in [0, 1].</returns>
    public static double EclipseFraction(Angle starAngularRadius, Angle occluderAngularRadius, Angle angularSeparation)
    {
        var r1 = starAngularRadius.Radians;
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
