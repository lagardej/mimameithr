using Brunnr.Component;
using Brunnr.Domain;
using Brunnr.Messaging;
using Brunnr.Schedule;
using Brunnr.Topology;
using Xunit;

namespace Brunnr.Tests;

public sealed class EngineTests
{
    private readonly InMemoryMessageBus _bus = new();
    private readonly InMemoryDomainStateStore<DummyState> _stateStore = new();
    private readonly IGrid _testGrid = new TestGrid();

    [Fact]
    public void Start_BootstrapsConfiguredComponentForDomain()
    {
        var componentRegistry = new ComponentRegistry();
        componentRegistry.Register(nameof(DummyComponent), new DummyComponent(_bus, _stateStore, _testGrid));

        var configuration = new EngineConfiguration()
            .BindComponent(new DomainId("test"), nameof(DummyComponent));

        Envelope<JobScheduled>? scheduled = null;
        _bus.Subscribe<JobScheduled>(e => scheduled ??= e);

        var engine = new Engine(_bus, componentRegistry);
        engine.Configure(configuration);
        engine.Start();

        Assert.NotNull(scheduled);
        Assert.Equal(new DomainId("test"), scheduled.Payload.Domain);
        Assert.Equal(nameof(DummyComponent), scheduled.Payload.ComponentId);
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
