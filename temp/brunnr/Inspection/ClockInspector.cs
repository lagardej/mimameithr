using Brunnr.Clock;
using Brunnr.Messaging;
using System.Diagnostics;

namespace Brunnr.Inspection;

/// <summary>
///     Tracks simulated time and derives wall-clock compression ratio.
///     Compression = simulated seconds elapsed / wall-clock seconds elapsed.
/// </summary>
public sealed class ClockInspector
{
    private readonly Func<TimeSpan> _wallClock;

    public ClockInspector(IMessageBus bus, Func<TimeSpan>? wallClock = null)
    {
        var sw = Stopwatch.StartNew();
        _wallClock = wallClock ?? (() => sw.Elapsed);
        bus.Subscribe<ClockTicked>(evt => ElapsedSeconds = evt.Payload.ElapsedSeconds);
    }

    public ulong ElapsedSeconds { get; private set; }
    public TimeSpan WallClockElapsed => _wallClock();

    /// <summary>Simulated seconds per wall-clock second. Zero until the first tick.</summary>
    public double CompressionRatio =>
        _wallClock().TotalSeconds > 0
            ? ElapsedSeconds / _wallClock().TotalSeconds
            : 0;
}
