namespace Kvasir.Science.Maths.Geometry;

/// <summary>An immutable 3D vector with double-precision components. Supports standard vector arithmetic.</summary>
public readonly record struct Vector3D(double X, double Y, double Z)
{
    /// <summary>Dot product of two vectors. Returns a scalar measuring directional alignment.</summary>
    public static double Dot(Vector3D a, Vector3D b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

    /// <summary>Component-wise addition of two vectors.</summary>
    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    /// <summary>Component-wise subtraction of two vectors.</summary>
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    /// <summary>Scalar multiplication: scales all components by <paramref name="s" />.</summary>
    public static Vector3D operator *(Vector3D v, double s) => new(v.X * s, v.Y * s, v.Z * s);

    /// <summary>Scalar multiplication: scales all components by <paramref name="s" />.</summary>
    public static Vector3D operator *(double s, Vector3D v) => v * s;
}
