using Kvasir;
using System.Numerics;
using UnitsNet;

namespace Nornir.Geimr.Geometry;

/// <summary>Static geometric utility methods for stellar position relative to a planetary surface.</summary>
public static class StellarGeometry
{
    /// <summary>
    ///     Computes the stellar zenith angle at a surface cell.
    ///     The star's direction is expressed in the planet's body frame, accounting for axial tilt
    ///     (seasonal effect) and the body's current rotation angle (diurnal effect).
    ///     The cell's surface normal is derived from its geographic position.
    /// </summary>
    /// <param name="position">Cell centre geographic position.</param>
    /// <param name="orbitalAngle">True anomaly — current angular position in the orbit.</param>
    /// <param name="rotationAngle">Current rotation angle of the body around its axis.</param>
    /// <param name="axialTilt">Angle between the body's rotation axis and its orbital plane normal.</param>
    /// <returns>Stellar zenith angle. Values above 90° indicate the night side.</returns>
    public static Angle StellarZenithAngle(LatLng position, Angle orbitalAngle, Angle rotationAngle, Angle axialTilt)
    {
        // Planet → star direction in the orbital plane.
        var starDirectionOrbital = new Vector3((float) -Math.Cos(orbitalAngle.Radians),
            (float) -Math.Sin(orbitalAngle.Radians), 0f);

        // Apply axial tilt (Y) then planetary rotation (Z).
        var starDir = Vector3.Transform(starDirectionOrbital, Matrix4x4.CreateRotationY((float) axialTilt.Radians));
        starDir = Vector3.Transform(starDir, Matrix4x4.CreateRotationZ((float) -rotationAngle.Radians));

        // Outward surface normal at the cell's geographic position.
        var normal = position.ToUnitVector();

        // Zenith angle between the local surface normal and the star direction.
        // cos θ = 1 => star at zenith
        // cos θ = 0 => terminator
        // cos θ < 0 => night side
        var cosTheta = Vector3.Dot(normal, starDir);

        return Angle.FromRadians(Math.Acos(Math.Clamp(cosTheta, -1f, 1f)));
    }
}
