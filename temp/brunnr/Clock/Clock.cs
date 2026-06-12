namespace Brunnr.Clock;

/// <summary>Monotonic simulation clock. Tracks elapsed simulated time in seconds since epoch.</summary>
public sealed class Clock
{
    public ulong ElapsedSeconds { get; private set; }

    internal void Advance(ulong delta) => ElapsedSeconds += delta;
}
