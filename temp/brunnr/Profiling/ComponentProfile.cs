namespace Brunnr.Profiling;

/// <summary>
///     Profiling data collected for a single component.
///     All durations are wall-clock time.
/// </summary>
public sealed class ComponentProfile(int windowSize)
{
    private readonly Queue<TimeSpan> _window = new(windowSize);

    // ── Total (since start) ──────────────────────────────────────────────────

    public long TickCount { get; private set; }
    public TimeSpan TotalDuration { get; private set; }
    public TimeSpan MinDuration { get; private set; } = TimeSpan.MaxValue;
    public TimeSpan MaxDuration { get; private set; } = TimeSpan.MinValue;
    public TimeSpan LastDuration { get; private set; }

    public TimeSpan AverageDuration =>
        TickCount == 0 ? TimeSpan.Zero : TotalDuration / TickCount;

    // ── Window (last N ticks) ────────────────────────────────────────────────

    public int WindowSize { get; } = windowSize;

    public TimeSpan WindowP50 => Percentile(0.50);
    public TimeSpan WindowP95 => Percentile(0.95);
    public TimeSpan WindowP99 => Percentile(0.99);

    // ── Internal ─────────────────────────────────────────────────────────────

    internal void Record(TimeSpan duration)
    {
        TickCount++;
        TotalDuration += duration;
        LastDuration = duration;

        if (duration < MinDuration)
        {
            MinDuration = duration;
        }

        if (duration > MaxDuration)
        {
            MaxDuration = duration;
        }

        if (_window.Count == WindowSize)
        {
            _window.Dequeue();
        }

        _window.Enqueue(duration);
    }

    private TimeSpan Percentile(double p)
    {
        if (_window.Count == 0)
        {
            return TimeSpan.Zero;
        }

        var sorted = _window.OrderBy(d => d).ToList();
        var index = (int) Math.Ceiling(p * sorted.Count) - 1;

        return sorted[Math.Max(0, index)];
    }
}
