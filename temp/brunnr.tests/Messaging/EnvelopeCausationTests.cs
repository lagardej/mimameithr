using Brunnr.Clock;
using Brunnr.Component;
using Brunnr.Dispatch;
using Brunnr.Domain;
using Brunnr.Messaging;
using Brunnr.Schedule;
using Brunnr.Topology;
using Xunit;

namespace Brunnr.Tests.Messaging;

/// <summary>
///     Verifies causation/correlation tracing across a full cascade: EngineStarted -> JobScheduled ->
///     JobsDue -> JobStarted -> ComponentTicked (+ rescheduled JobScheduled).
///     NOTE: per-job CorrelationId traces back to when the job was *scheduled*, not to the ClockTicked
///     that triggered its execution. JobStarted is CausedBy the job's own JobScheduled envelope (not the
///     batched JobsDue envelope), so a job's whole execution chain shares its original scheduling's
///     CorrelationId — see docs/envelope-correlation-refactor.adoc.
/// </summary>
public sealed class EnvelopeCausationTests
{
    private readonly InMemoryMessageBus _bus = new();
    private readonly ComponentRegistry _componentRegistry = new();

    private readonly EngineConfiguration _configuration = new EngineConfiguration()
        .BindComponent(new DomainId("test"), nameof(DummyComponent));

    private readonly InMemoryDomainStateStore<DummyState> _stateStore = new();
    private readonly IGrid _testGrid = new TestGrid();

    [Fact]
    public void Cascade_PreservesPerJobCorrelationFromScheduling_NotFromTheTriggeringTick()
    {
        _ = new Scheduler(_bus);
        _ = new Dispatcher(_bus);
        var component = new DummyComponent(_bus, _stateStore, _testGrid);
        _componentRegistry.Register(nameof(DummyComponent), component);

        Envelope<JobScheduled>? scheduled = null;
        Envelope<JobStarted>? started = null;
        Envelope<ComponentTicked>? ticked = null;

        _bus.Subscribe<JobScheduled>(e => scheduled ??= e);
        _bus.Subscribe<JobStarted>(e => started = e);
        _bus.Subscribe<ComponentTicked>(e => ticked = e);

        var engineStarted = Envelope<EngineStarted>.Root(new EngineStarted());
        _bus.Publish(engineStarted);

        // Bootstrap: schedule component for domain on EngineStarted
        foreach (var domainConfig in _configuration.Domains)
        {
            foreach (var (componentName, _) in domainConfig.Bindings)
            {
                var comp = _componentRegistry.Get(componentName);
                if (comp != null)
                {
                    comp.ScheduleFor(domainConfig.Id, 0, engineStarted, CellSet.Global);
                }
            }
        }

        Assert.NotNull(scheduled);
        Assert.Equal(engineStarted.CorrelationId, scheduled.CorrelationId);
        Assert.Equal(engineStarted.MessageId, scheduled.CausationId);

        var clockTicked = Envelope<ClockTicked>.Root(new ClockTicked(0));
        _bus.Publish(clockTicked);

        Assert.NotNull(started);
        Assert.NotNull(ticked);

        // JobStarted is caused by the job's own JobScheduled envelope, not by the ClockTicked/JobsDue
        // envelope that triggered dispatch this tick.
        Assert.Equal(scheduled.CorrelationId, started.CorrelationId);
        Assert.Equal(scheduled.MessageId, started.CausationId);
        Assert.NotEqual(clockTicked.CorrelationId, started.CorrelationId);

        // ComponentTicked is caused by JobStarted (one hop).
        Assert.Equal(started.CorrelationId, ticked.CorrelationId);
        Assert.Equal(started.MessageId, ticked.CausationId);

        // The whole chain, from EngineStarted through to ComponentTicked, shares one CorrelationId.
        Assert.Equal(engineStarted.CorrelationId, ticked.CorrelationId);
    }

    [Fact]
    public void Root_HasNoCausation()
    {
        var envelope = Envelope<ClockTicked>.Root(new ClockTicked(0));

        Assert.Null(envelope.CausationId);
    }

    [Fact]
    public void CausedBy_PreservesCorrelationAndSetsCausationToParentMessageId()
    {
        var cause = Envelope<ClockTicked>.Root(new ClockTicked(0));
        var effect = cause.CausedBy(new JobsDue([], 0));

        Assert.Equal(cause.CorrelationId, effect.CorrelationId);
        Assert.Equal(cause.MessageId, effect.CausationId);
        Assert.NotEqual(cause.MessageId, effect.MessageId);
    }

    private sealed class TestGrid : IGrid
    {
        public CellId[] RootCells() => throw new NotSupportedException();
        public CellId CellAt(double latDeg, double lngDeg, Resolution resolution) => throw new NotSupportedException();
        public IEnumerable<CellId> CellsAtResolution(Resolution resolution) => throw new NotSupportedException();
        public Resolution ResolutionOf(CellId cell) => throw new NotSupportedException();
        public LatLng CenterOf(CellId cell) => throw new NotSupportedException();
        public LatLng[] BoundaryOf(CellId cell) => throw new NotSupportedException();
        public bool IsValid(CellId cell) => true;
        public CellId ParentOf(CellId cell, Resolution parentResolution) => throw new NotSupportedException();
        public CellId[] ChildrenOf(CellId cell, Resolution childResolution) => throw new NotSupportedException();
        public Resolution InferResolution<T>(Dictionary<CellId, T> cells, Resolution fallback) => fallback;
        public CellId[] Disk(CellId cell, int k) => throw new NotSupportedException();
        public IEnumerable<CellId> GridRing(CellId center, int k) => throw new NotSupportedException();
    }
}
