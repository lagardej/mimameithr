using Brunnr.Clock;
using Brunnr.Component;
using Brunnr.Inspection;
using Brunnr.Messaging;
using Xunit;

namespace Brunnr.Tests.Inspection;

public sealed class BusMonitorTests
{
    private readonly InMemoryMessageBus _inner = new();
    private readonly BusMonitor _monitor;

    public BusMonitorTests() => _monitor = new BusMonitor(_inner);

    [Fact]
    public void Counts_EmptyBeforeAnyPublish() => Assert.Empty(_monitor.Counts);

    [Fact]
    public void Counts_TracksPublishByTypeName()
    {
        _monitor.Publish(Envelope<ClockTicked>.Root(new ClockTicked(1)));

        Assert.Equal(1L, _monitor.Counts[nameof(ClockTicked)]);
    }

    [Fact]
    public void Counts_AccumulatesMultiplePublishesOfSameType()
    {
        _monitor.Publish(Envelope<ClockTicked>.Root(new ClockTicked(1)));
        _monitor.Publish(Envelope<ClockTicked>.Root(new ClockTicked(2)));
        _monitor.Publish(Envelope<ClockTicked>.Root(new ClockTicked(3)));

        Assert.Equal(3L, _monitor.Counts[nameof(ClockTicked)]);
    }

    [Fact]
    public void Counts_TracksMultipleTypesIndependently()
    {
        _monitor.Publish(Envelope<ClockTicked>.Root(new ClockTicked(1)));
        _monitor.Publish(Envelope<ClockTicked>.Root(new ClockTicked(2)));
        _monitor.Publish(Envelope<ComponentTicked>.Root(new ComponentTicked("c", 1, TimeSpan.Zero, [])));

        Assert.Equal(2L, _monitor.Counts[nameof(ClockTicked)]);
        Assert.Equal(1L, _monitor.Counts[nameof(ComponentTicked)]);
    }

    [Fact]
    public void Publish_ForwardsToInnerBus()
    {
        var received = false;
        _inner.Subscribe<ClockTicked>(_ => received = true);

        _monitor.Publish(Envelope<ClockTicked>.Root(new ClockTicked(1)));

        Assert.True(received);
    }

    [Fact]
    public void Subscribe_ForwardsToInnerBus()
    {
        var received = false;
        _monitor.Subscribe<ClockTicked>(_ => received = true);

        _inner.Publish(Envelope<ClockTicked>.Root(new ClockTicked(1)));

        Assert.True(received);
    }
}
