using UnitsNet;

namespace Kvasir.Science.Maths.Geometry.Spherics;

/// <summary>Pure spherical geometry functions.</summary>
public static class SphericalGeometry
{
    /// <summary>Returns the dot product of two 3D vectors.</summary>
    public static double Dot(Coords3D a, Coords3D b) =>
        (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

    /// <summary>
    ///     Returns the current rotation angle of the domain around its axis, in radians.
    ///     Returns zero if the rotation period is zero (tidally locked or non-rotating body).
    /// </summary>
    public static double RotationAngle(Duration rotationPeriod, ulong elapsedSeconds) =>
        rotationPeriod.Seconds > 0.0
            ? 2.0 * Math.PI * (elapsedSeconds % rotationPeriod.Seconds) / rotationPeriod.Seconds
            : 0.0;
}
