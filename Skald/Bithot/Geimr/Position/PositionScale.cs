using Godot;
using UnitsNet;

namespace Skald.Bithot.Geimr.Position;

/// <summary>
///     Maps real Cartesian position to scene-space <see cref="Vector3" />. Linear, unlike
///     <see cref="Geimr.Geometry.VisualScale" />'s log compression for radius — orbital distances in this
///     system span a much narrower range (fractions of an AU to tens of AU) than body radii do (asteroid to
///     supergiant), so a fixed divisor keeps relative distances and orbit shapes undistorted.
/// </summary>
public static class PositionScale
{
    /// <summary>
    ///     Kilometres per scene unit. Chosen so 1 AU (~150,000,000 km) maps to 50 scene units — inside
    ///     <see cref="Orbit.OrbitCamera" />'s default view distance for a sun-sized body.
    /// </summary>
    private const double KmPerSceneUnit = 3_000_000.0;

    /// <summary>Maps a physical position to a scene-space position via linear scale.</summary>
    public static Vector3 ToVisualPosition(Length x, Length y, Length z)
    {
        return new Vector3(
            (float)(x.Kilometers / KmPerSceneUnit),
            (float)(y.Kilometers / KmPerSceneUnit),
            (float)(z.Kilometers / KmPerSceneUnit));
    }
}