namespace Brunnr.Engine.Time;

/// <summary>Monotonic simulation clock. Tracks elapsed simulated time in seconds since epoch.</summary>
public sealed class MonotonicClock
{
    /// <summary>
    ///     The number of seconds since Epoch
    /// </summary>
    public float ElapsedSeconds { get; private set; }

    public void Advance(float delta) => ElapsedSeconds += delta;
}
