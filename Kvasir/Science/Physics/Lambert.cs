using Kvasir.Science.Maths.Geometry;

namespace Kvasir.Science.Physics;

/// <summary>Lambert's cosine law: irradiance on a surface scales with the cosine of the angle of incidence.</summary>
public static class Lambert
{
    /// <summary>
    ///     Returns the cosine of the angle between the surface normal and the incoming light direction.
    ///     1.0 = light hits head-on, 0.0 = terminator, negative = surface faces away from the light source.
    ///     Unitless.
    /// </summary>
    public static double CosineFactor(Vector3D normal, Vector3D lightDirection) =>
        Vector3D.Dot(normal, lightDirection);
}
