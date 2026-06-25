namespace Brunnr.Engine.Clock;

/// <summary>Carries the current simulated time since epoch and the duration of this tick.</summary>
public record ClockTicked(ulong ElapsedSeconds, ulong TickDelta);

/// <summary>Monotonic simulation clock. Tracks elapsed simulated time in seconds since epoch.</summary>
public sealed class MonotonicClock
{
    /// <summary>
    ///     The number of seconds since Epoch
    /// </summary>
    public float ElapsedSeconds { get; private set; }

    public void Advance(float delta) => ElapsedSeconds += delta;
}
