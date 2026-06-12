using Brunnr.Clock;
using Brunnr.Component;
using Brunnr.Dispatch;
using Brunnr.Domain;
using Brunnr.Messaging;
using Brunnr.Schedule;
using Brunnr.Topology;
using Xunit;

namespace Brunnr.Tests.Scheduling;

public sealed class SchedulerTests
{
    private readonly InMemoryMessageBus _bus = new();
    private readonly ComponentRegistry _componentRegistry;
    private readonly EngineConfiguration _configuration;
    private readonly Dispatcher _dispatcher;
    private readonly InMemoryDomainStateStore<DummyState> _stateStore = new();
    private readonly IGrid _testGrid = new TestGrid();

    public SchedulerTests()
    {
        _dispatcher = new Dispatcher(_bus);
        _componentRegistry = new ComponentRegistry();
        _configuration = new EngineConfiguration()
            .BindComponent(new DomainId("test"), nameof(DummyComponent));
    }

    private void StartEngine()
    {
        var engineStartedEnvelope = Envelope<EngineStarted>.Root(new EngineStarted());
        _bus.Publish(engineStartedEnvelope);

        // Bootstrap components
        foreach (var domainConfig in _configuration.Domains)
        {
            foreach (var (componentName, _) in domainConfig.Bindings)
            {
                var component = _componentRegistry.Get(componentName);
                if (component != null)
                {
                    component.ScheduleFor(domainConfig.Id, 0, engineStartedEnvelope, CellSet.Global);
                }
            }
        }
    }

    private void Tick(ulong elapsedSeconds) =>
        _bus.Publish(Envelope<ClockTicked>.Root(new ClockTicked(elapsedSeconds)));

    [Fact]
    public void PendingJobCount_ReflectsEnqueuedJobs()
    {
        var scheduler = new Scheduler(_bus);
        var component = new DummyComponent(_bus, _stateStore, _testGrid);
        _componentRegistry.Register(nameof(DummyComponent), component);
        StartEngine();

        Assert.Equal(1, scheduler.PendingJobCount);
    }

    [Fact]
    public void PendingJobCount_IsZeroBeforeEngineStarted()
    {
        var scheduler = new Scheduler(_bus);
        _ = new DummyComponent(_bus, _stateStore, _testGrid);

        Assert.Equal(0, scheduler.PendingJobCount);
    }

    [Fact]
    public void IntakeQueue_ContainsJobBeforeClockTick()
    {
        var scheduler = new Scheduler(_bus);
        var component = new DummyComponent(_bus, _stateStore, _testGrid);
        _componentRegistry.Register(nameof(DummyComponent), component);
        StartEngine();

        Assert.Single(scheduler.IntakeQueue);
    }

    [Fact]
    public void HeapSnapshot_ContainsJobAfterClockTick()
    {
        var scheduler = new Scheduler(_bus);
        var component = new DummyComponent(_bus, _stateStore, _testGrid);
        _componentRegistry.Register(nameof(DummyComponent), component);
        StartEngine();

        Tick(1);

        Assert.Empty(scheduler.HeapSnapshot);
        Assert.Single(scheduler.IntakeQueue);
    }

    [Fact]
    public void HeapSnapshot_ContainsJobOnceDrained()
    {
        var scheduler = new Scheduler(_bus);
        var component = new DummyComponent(_bus, _stateStore, _testGrid);
        _componentRegistry.Register(nameof(DummyComponent), component);
        StartEngine();

        Tick(1);
        Tick(2);

        Assert.Single(scheduler.HeapSnapshot);
        Assert.Equal(11ul, scheduler.HeapSnapshot[0].DueAt);
    }

    [Fact]
    public void JobsDueAt_ReturnsJobsDueAtOrBefore()
    {
        var scheduler = new Scheduler(_bus);
        var component = new DummyComponent(_bus, _stateStore, _testGrid);
        _componentRegistry.Register(nameof(DummyComponent), component);
        StartEngine();

        Tick(1);

        var due = scheduler.JobsDueAt(11);
        Assert.Single(due);
    }

    [Fact]
    public void JobsDueAt_ReturnsEmptyWhenNoneDue()
    {
        var scheduler = new Scheduler(_bus);
        _ = new DummyComponent(_bus, _stateStore, _testGrid);
        StartEngine();

        Tick(1);

        var due = scheduler.JobsDueAt(0);
        Assert.Empty(due);
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
