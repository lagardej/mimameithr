namespace Brunnr.Profiling;

/// <summary>
///     Profiling data collected for a single DynaCore metric.
///     Tracks the same rolling-window statistics as <see cref="ComponentProfile" /> but over scalar values.
/// </summary>
public sealed class DynaCoreProfile(int windowSize)
{
    private readonly Queue<double> _window = new(windowSize);

    // ── Total (since start) ──────────────────────────────────────────────────

    public long SampleCount { get; private set; }
    public double Total { get; private set; }
    public double Min { get; private set; } = double.MaxValue;
    public double Max { get; private set; } = double.MinValue;
    public double Last { get; private set; }

    public double Average => SampleCount == 0 ? 0d : Total / SampleCount;

    // ── Window (last N samples) ──────────────────────────────────────────────

    public int WindowSize { get; } = windowSize;

    public double WindowP50 => Percentile(0.50);
    public double WindowP95 => Percentile(0.95);
    public double WindowP99 => Percentile(0.99);

    // ── Internal ─────────────────────────────────────────────────────────────

    internal void Record(double value)
    {
        SampleCount++;
        Total += value;
        Last = value;

        if (value < Min)
        {
            Min = value;
        }

        if (value > Max)
        {
            Max = value;
        }

        if (_window.Count == WindowSize)
        {
            _window.Dequeue();
        }

        _window.Enqueue(value);
    }

    private double Percentile(double p)
    {
        if (_window.Count == 0)
        {
            return 0d;
        }

        var sorted = _window.OrderBy(v => v).ToList();
        var index = (int) Math.Ceiling(p * sorted.Count) - 1;

        return sorted[Math.Max(0, index)];
    }
}
