namespace Kjarni.Kvasir.Foundation;

/// <summary>Interpolation helpers for scalar values.</summary>
public static class Interpolation
{
    /// <summary>Linear interpolation between <paramref name="a" /> and <paramref name="b" /> for t in [0, 1].</summary>
    public static double Lerp(double a, double b, double t) => a + ((b - a) * Math.Clamp(t, 0d, 1d));
}
