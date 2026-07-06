using UnitsNet;

namespace Kjarni.Kvasir.Geimr;

/// <summary>Rotation state functions for a planetary body.</summary>
public static class BodyRotation
{
    /// <summary>Computes the current rotation angle of a body around its axis.</summary>
    /// <param name="epochAngle">Rotation angle at epoch (t = 0).</param>
    /// <param name="rotationRate">Rate at which the body completes a full rotation.</param>
    /// <param name="elapsedTime">Total elapsed time since epoch.</param>
    /// <returns>Current rotation angle, wrapped to [0°, 360°).</returns>
    public static Angle CurrentAngle(Angle epochAngle, RotationalSpeed rotationRate, Duration elapsedTime) =>
        Angle.FromDegrees((epochAngle.Degrees + (rotationRate.DegreesPerSecond * elapsedTime.Seconds)) % 360.0);
}
