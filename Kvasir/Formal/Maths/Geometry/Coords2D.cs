namespace Kvasir.Formal.Maths.Geometry;

/// <summary>
///     A 2D position with double-precision components.
///     Represented as a displacement vector from the origin — can be used directly as a <see cref="Vector3D" />.
/// </summary>
public readonly record struct Coords2D(double X, double Y)
{
    /// <summary>Implicitly converts a position to its equivalent displacement vector from the origin.</summary>
    public static implicit operator Vector2D(Coords2D c) => new(c.X, c.Y);
}
