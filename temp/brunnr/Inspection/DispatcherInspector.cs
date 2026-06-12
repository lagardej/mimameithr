using Brunnr.Clock;
using Brunnr.Component;
using Brunnr.Dispatch;
using Brunnr.Messaging;

namespace Brunnr.Inspection;

/// <summary>
///     Tracks per-tick job dispatch counts and observed parallelism.
///     Parallelism is measured as the peak number of jobs overlapping in wall-clock time within a single tick.
/// </summary>
public sealed class DispatcherInspector
{
    private int _activeJobs;
    private int _dispatchedThisTick;
    private int _peakThisTick;
    private long _totalJobsDispatched;

    public DispatcherInspector(IMessageBus bus)
    {
        bus.Subscribe<ClockTicked>(_ => OnClockTicked());
        bus.Subscribe<JobStarted>(_ => OnJobStarted());
        bus.Subscribe<ComponentTicked>(_ => OnComponentTicked());
    }

    public int JobsDispatchedLastTick { get; private set; }
    public int PeakConcurrencyLastTick { get; private set; }
    public long TotalJobsDispatched => _totalJobsDispatched;

    private void OnClockTicked()
    {
        JobsDispatchedLastTick = _dispatchedThisTick;
        PeakConcurrencyLastTick = _peakThisTick;
        _dispatchedThisTick = 0;
        _peakThisTick = 0;
    }

    private void OnJobStarted()
    {
        Interlocked.Increment(ref _dispatchedThisTick);
        Interlocked.Increment(ref _totalJobsDispatched);
        var current = Interlocked.Increment(ref _activeJobs);
        InterlockedMax(ref _peakThisTick, current);
    }

    private void OnComponentTicked() => Interlocked.Decrement(ref _activeJobs);

    private static void InterlockedMax(ref int location, int candidate)
    {
        int current;
        do
        {
            current = location;
            if (candidate <= current)
            {
                return;
            }
        } while (Interlocked.CompareExchange(ref location, candidate, current) != current);
    }
}
