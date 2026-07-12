using System.Numerics;

namespace Kvasir;

/// <summary>
///     A 3D position with double-precision components.
///     Represented as a displacement vector from the origin — can be used directly as a <see cref="Vector3" />.
/// </summary>
public readonly record struct Coords3D(float X, float Y, float Z)
{
    /// <summary>Implicitly converts a position to its equivalent displacement vector from the origin.</summary>
    public static implicit operator Vector3(Coords3D c) => new(c.X, c.Y, c.Z);
}

/// <summary>
///     A 2D position with float-precision components.
///     Represented as a displacement vector from the origin — can be used directly as a <see cref="Vector3" />.
/// </summary>
public readonly record struct Coords2D(float X, float Y)
{
    /// <summary>Implicitly converts a position to its equivalent displacement vector from the origin.</summary>
    public static implicit operator Vector2(Coords2D c) => new(c.X, c.Y);
}
