using Xunit;

namespace Nornir.Tests;

internal static class AssertEx
{
    /// <summary>
    ///     Asserts that <paramref name="actual" /> is within <paramref name="epsilon" /> of <paramref name="expected" />,
    ///     expressed as an absolute margin. Works correctly at any magnitude, including zero.
    ///     Default epsilon of 1e-6 is suitable for most floating-point comparisons at simulation scale.
    /// </summary>
    internal static void Close(double expected, double actual, double epsilon = 1e-6) =>
        Assert.InRange(actual, expected - epsilon, expected + epsilon);
}
