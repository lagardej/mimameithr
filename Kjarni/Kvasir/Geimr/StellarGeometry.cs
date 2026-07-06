using Kjarni.Kvasir.Foundation;
using UnitsNet;

namespace Kjarni.Kvasir.Geimr;

/// <summary>Stellar geometry functions for computing a star's position, flux, and eclipses at a surface cell.</summary>
public static class StellarGeometry
{
    /// <summary>
    ///     Computes the stellar zenith angle at a surface cell.
    ///     The star's direction is expressed in the planet's body frame, accounting for axial tilt
    ///     (seasonal effect) and the body's current rotation angle (diurnal effect).
    ///     The cell's surface normal is derived from its geographic position.
    ///     Steps:
    ///     1. Express the star's direction in the orbital plane (unit vector from planet to star).
    ///     2. Rotate by axial tilt to convert to body frame — tilting the pole toward/away from the star
    ///     produces the seasonal variation in insolation.
    ///     3. Compute the cell's outward surface normal from lat/lng.
    ///     4. The zenith angle is the arc-cosine of the dot product of the two unit vectors.
    ///     cos θ = 1 → star directly overhead; cos θ = 0 → terminator; cos θ &lt; 0 → night side.
    /// </summary>
    /// <param name="position">Cell centre geographic position.</param>
    /// <param name="orbitalAngle">True anomaly — current angular position in the orbit.</param>
    /// <param name="rotationAngle">Current rotation angle of the body around its axis.</param>
    /// <param name="axialTilt">Angle between the body's rotation axis and its orbital plane normal.</param>
    /// <returns>Stellar zenith angle. Values above 90° indicate the night side.</returns>
    public static Angle StellarZenithAngle(LatLng position, Angle orbitalAngle, Angle rotationAngle, Angle axialTilt)
    {
        var starDir = ApplyRotation(ApplyAxialTilt(StarDirectionOrbital(orbitalAngle), axialTilt), rotationAngle);
        var normal = position.ToUnitVector();
        var cosTheta = Vector3D.Dot(normal, starDir);
        return Angle.FromRadians(Math.Acos(Math.Clamp(cosTheta, -1.0, 1.0)));
    }

    /// <summary>Computes the stellar flux at a surface cell. Returns zero on the night side.</summary>
    /// <param name="luminosity">Total power radiated by the star across all wavelengths.</param>
    /// <param name="distanceFromStar">Distance between the body and its parent star.</param>
    /// <param name="zenithAngle">Stellar zenith angle at the cell.</param>
    /// <returns>Incoming stellar flux at the cell's surface.</returns>
    public static Irradiance Insolation(Luminosity luminosity, Length distanceFromStar, Angle zenithAngle)
    {
        var fluxWattsPerM2 = luminosity.Watts / (4.0 * Math.PI * distanceFromStar.Meters * distanceFromStar.Meters);
        var watts = zenithAngle.Degrees < 90.0 ? fluxWattsPerM2 * Math.Cos(zenithAngle.Radians) : 0.0;
        return Irradiance.FromWattsPerSquareMeter(watts);
    }

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

    /// <summary>Star direction in the orbital plane (planet → star, unit vector).</summary>
    private static Vector3D StarDirectionOrbital(Angle orbitalAngle) =>
        new(-Math.Cos(orbitalAngle.Radians), -Math.Sin(orbitalAngle.Radians), 0.0);

    /// <summary>Rotates the star direction into the body frame by axial tilt (rotation around Y axis).</summary>
    private static Vector3D ApplyAxialTilt(Vector3D star, Angle axialTilt)
    {
        var tilt = axialTilt.Radians;
        return new Vector3D(
            star.X * Math.Cos(tilt),
            star.Y,
            -star.X * Math.Sin(tilt));
    }

    /// <summary>Rotates the star direction around Z axis to account for body spin.</summary>
    private static Vector3D ApplyRotation(Vector3D star, Angle rotationAngle)
    {
        var rot = -rotationAngle.Radians;
        return new Vector3D(
            (star.X * Math.Cos(rot)) - (star.Y * Math.Sin(rot)),
            (star.X * Math.Sin(rot)) + (star.Y * Math.Cos(rot)),
            star.Z);
    }
}
