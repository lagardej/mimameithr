namespace Brunnr.Engine.Clock;

/// <summary>Carries the current simulated time since epoch and the duration of this tick.</summary>
public record ClockTicked(ulong ElapsedSeconds, ulong TickDelta);

/// <summary>Monotonic simulation clock. Tracks elapsed simulated time in seconds since epoch.</summary>
public sealed class MonotonicClock
{
    /// <summary>
    ///     The number of seconds since Epoch
    /// </summary>
    public ulong ElapsedSeconds { get; private set; }

    internal void Advance(ulong delta) => ElapsedSeconds += delta;
}
