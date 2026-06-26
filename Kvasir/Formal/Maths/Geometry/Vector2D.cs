namespace Kvasir.Formal.Maths.Geometry;

/// <summary>
///     An immutable 2D vector with double-precision components.
///     Supports standard vector arithmetic.
/// </summary>
public readonly record struct Vector2D(double X, double Y)
{
    /// <summary>Implicitly converts the 2D vector to its equivalent 3D vector from the origin.</summary>
    public static implicit operator Vector3D(Vector2D v) => new(v.X, v.Y, 0);
}
