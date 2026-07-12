using Kjarni.Nornir.Geimr.Rotation;
using Xunit;

namespace Kjarni.Nornir.Tests;

/// <summary>
///     Regression coverage for the <c>Tick.time</c> vs <c>Tick.deltaTime</c> bug in
///     <see cref="RotationSystem" />: using absolute sim-clock time instead of the elapsed
///     time since the previous fire compounded the rotation angle on every stagger fire.
/// </summary>
public class RotationSystemTests
{
    [Fact]
    public void CurrentAngle_MatchesPhysicalRotation_AcrossNonUniformStaggeredIntervals()
    {
        var nornir = new Nornir();
        var id = nornir.CreateEntity();

        // RotationPeriod scale 1000 -> 10 s period (see SetRotation doc table) -> 36 deg/s.
        nornir.Handle(new SetRotation(id, 1, 1000));

        // Non-uniform Advance() calls straddling the 10s stagger interval twice (fires at
        // cumulative 11s and again at cumulative 22s), rather than one call per interval.
        foreach (var delta in new[] { 6f, 5f, 4f, 7f })
        {
            nornir.Advance(delta);
        }

        const double totalElapsedSeconds = 22.0; // 6+5+4+7
        const double degreesPerSecond = 36.0; // 360 / 10s period
        var expectedDegrees = (1.0 + (degreesPerSecond * totalElapsedSeconds)) % 360.0; // 73°

        var actualDegrees = nornir.GetComponent<RotationC>(id).CurrentAngle.Degrees;

        // Regression check: the pre-fix (Tick.time-based) computation would have compounded
        // the second fire's elapsed time as if it were absolute, yielding 109° instead of 73°.
        AssertEx.Close(expectedDegrees, actualDegrees);
    }
}
