using Kjarni.Kvasir.Formal.Maths.Geometry;
using Kjarni.Kvasir.Natural.Physical.Geodesy;
using UnitsNet;

namespace Kjarni.Kvasir.Natural.Physical.Physics;

/// <summary>Solar geometry functions for computing sun position and flux at a surface cell.</summary>
[Module("Natural/Physical/Physics", "Sun direction, zenith angle, and insolation at a surface cell.")]
public static class SolarGeometry
{
    /// <summary>
    ///     Returns the current rotation angle of a body around its axis, in radians.
    ///     Returns zero for tidally locked or non-rotating bodies.
    /// </summary>
    /// <param name="rotationPeriod">Sidereal rotation period.</param>
    /// <param name="elapsedSeconds">Total elapsed simulation time in seconds.</param>
    public static double RotationAngle(Duration rotationPeriod, ulong elapsedSeconds) =>
        rotationPeriod.Seconds > 0.0
            ? 2.0 * Math.PI * (elapsedSeconds % rotationPeriod.Seconds) / rotationPeriod.Seconds
            : 0.0;

    /// <summary>
    ///     Computes the solar zenith angle at a surface cell.
    ///     The sun direction is expressed in the planet's body frame, accounting for axial tilt
    ///     (seasonal effect) and the body's current rotation angle (diurnal effect).
    ///     The cell's surface normal is derived from its geographic position.
    ///     Steps:
    ///     1. Express the sun's direction in the orbital plane (unit vector from planet to star).
    ///     2. Rotate by axial tilt to convert to body frame — tilting the pole toward/away from the sun
    ///     produces the seasonal variation in insolation.
    ///     3. Compute the cell's outward surface normal from lat/lng.
    ///     4. The zenith angle is the arc-cosine of the dot product of the two unit vectors.
    ///     cos θ = 1 → sun directly overhead; cos θ = 0 → terminator; cos θ &lt; 0 → night side.
    /// </summary>
    /// <param name="position">Cell centre geographic position.</param>
    /// <param name="orbitalAngle">True anomaly — current angular position in the orbit.</param>
    /// <param name="rotationAngle">Current rotation angle of the body around its axis.</param>
    /// <param name="axialTilt">Angle between the body's rotation axis and its orbital plane normal.</param>
    /// <returns>Solar zenith angle. Values above 90° indicate the night side.</returns>
    public static Angle SolarZenithAngle(LatLng position, Angle orbitalAngle, Angle rotationAngle, Angle axialTilt)
    {
        var sunDir = ApplyRotation(ApplyAxialTilt(SunDirectionOrbital(orbitalAngle), axialTilt), rotationAngle);
        var normal = SurfaceNormal(position);
        var cosTheta = Vector3D.Dot(normal, sunDir);
        return Angle.FromRadians(Math.Acos(Math.Clamp(cosTheta, -1.0, 1.0)));
    }

    /// <summary>
    ///     Computes the solar flux at a surface cell.
    ///     Returns zero on the night side.
    /// </summary>
    /// <param name="luminosity">Total power radiated by the star across all wavelengths.</param>
    /// <param name="distanceFromStar">Distance between the body and its parent star.</param>
    /// <param name="zenithAngle">Solar zenith angle at the cell.</param>
    /// <returns>Incoming solar flux at the cell's surface.</returns>
    public static Irradiance Insolation(Luminosity luminosity, Length distanceFromStar, Angle zenithAngle)
    {
        var fluxWattsPerM2 = luminosity.Watts / (4.0 * Math.PI * distanceFromStar.Meters * distanceFromStar.Meters);
        var watts = zenithAngle.Degrees < 90.0 ? fluxWattsPerM2 * Math.Cos(zenithAngle.Radians) : 0.0;
        return Irradiance.FromWattsPerSquareMeter(watts);
    }

    /// <summary>Sun direction in the orbital plane (planet → star, unit vector).</summary>
    private static Vector3D SunDirectionOrbital(Angle orbitalAngle) =>
        new(-Math.Cos(orbitalAngle.Radians), -Math.Sin(orbitalAngle.Radians), 0.0);

    /// <summary>Rotates the sun direction into the body frame by axial tilt (rotation around Y axis).</summary>
    private static Vector3D ApplyAxialTilt(Vector3D sun, Angle axialTilt)
    {
        var tilt = axialTilt.Radians;
        return new Vector3D(
            sun.X * Math.Cos(tilt),
            sun.Y,
            -sun.X * Math.Sin(tilt));
    }

    /// <summary>Rotates the sun direction around Z axis to account for body spin.</summary>
    private static Vector3D ApplyRotation(Vector3D sun, Angle rotationAngle)
    {
        var rot = -rotationAngle.Radians;
        return new Vector3D(
            (sun.X * Math.Cos(rot)) - (sun.Y * Math.Sin(rot)),
            (sun.X * Math.Sin(rot)) + (sun.Y * Math.Cos(rot)),
            sun.Z);
    }

    /// <summary>Outward surface normal at a geographic position.</summary>
    private static Vector3D SurfaceNormal(LatLng position)
    {
        var lat = position.Lat * Math.PI / 180.0;
        var lng = position.Lng * Math.PI / 180.0;
        return new Vector3D(
            Math.Cos(lat) * Math.Cos(lng),
            Math.Cos(lat) * Math.Sin(lng),
            Math.Sin(lat));
    }
}
