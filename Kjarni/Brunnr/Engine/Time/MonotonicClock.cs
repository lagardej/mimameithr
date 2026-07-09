namespace Kjarni.Brunnr.Engine.Time;

/// <summary>Monotonic simulation clock. Tracks elapsed simulated time in seconds since epoch.</summary>
public sealed class MonotonicClock
{
    /// <summary>
    ///     The number of seconds since Epoch
    /// </summary>
    public float ElapsedSeconds { get; private set; }

    /// <summary>Advances the clock by the specified delta time in seconds.</summary>
    /// <param name="delta">Number of seconds to advance.</param>
    public void Advance(float delta) => ElapsedSeconds += delta;
}
