using Brunnr.Component;
using Brunnr.Dynacore;
using Brunnr.Messaging;
using Brunnr.Profiling;
using Xunit;

namespace Brunnr.Tests.Profiling;

public sealed class DynaCoreProfilerTests
{
    private readonly InMemoryMessageBus _bus = new();

    private static Envelope<ComponentTicked> Evt(string componentId, params DynaCoreMetric[] metrics)
        => Envelope<ComponentTicked>.Root(new ComponentTicked(componentId, 0, TimeSpan.Zero, metrics));

    // ── Registration ─────────────────────────────────────────────────────────

    [Fact]
    public void Profiles_EmptyBeforeAnyEvent()
    {
        var profiler = new DynaCoreProfiler(_bus);

        Assert.Empty(profiler.Profiles);
    }

    [Fact]
    public void Profiles_EmptyWhenNoMetrics()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Envelope<ComponentTicked>.Root(new ComponentTicked("A", 0, TimeSpan.Zero, [])));

        Assert.Empty(profiler.Profiles);
    }

    [Fact]
    public void Profiles_CreatesEntryOnFirstMetric()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));

        Assert.True(profiler.Profiles.ContainsKey(("A", "temperature")));
    }

    [Fact]
    public void Profiles_TracksMultipleMetrics()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d), new DynaCoreMetric("pressure", 1.0d)));

        Assert.Equal(2, profiler.Profiles.Count);
    }

    [Fact]
    public void Profiles_TracksMetricsAcrossComponents()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));
        _bus.Publish(Evt("B", new DynaCoreMetric("temperature", 280d)));

        Assert.True(profiler.Profiles.ContainsKey(("A", "temperature")));
        Assert.True(profiler.Profiles.ContainsKey(("B", "temperature")));
    }

    // ── Stats ─────────────────────────────────────────────────────────────────

    [Fact]
    public void SampleCount_IncrementsPerEvent()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 310d)));

        Assert.Equal(2, profiler.Profiles[("A", "temperature")].SampleCount);
    }

    [Fact]
    public void Last_ReflectsMostRecentValue()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 310d)));

        Assert.Equal(310d, profiler.Profiles[("A", "temperature")].Last);
    }

    [Fact]
    public void Min_TracksSmallestValue()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 280d)));
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 310d)));

        Assert.Equal(280d, profiler.Profiles[("A", "temperature")].Min);
    }

    [Fact]
    public void Max_TracksLargestValue()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 280d)));
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 310d)));

        Assert.Equal(310d, profiler.Profiles[("A", "temperature")].Max);
    }

    [Fact]
    public void Average_IsCorrect()
    {
        var profiler = new DynaCoreProfiler(_bus);

        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 200d)));
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 400d)));

        Assert.Equal(300d, profiler.Profiles[("A", "temperature")].Average);
    }

    // ── Window / percentiles ─────────────────────────────────────────────────

    [Fact]
    public void WindowSize_DefaultIs1000()
    {
        var profiler = new DynaCoreProfiler(_bus);
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));

        Assert.Equal(1000, profiler.Profiles[("A", "temperature")].WindowSize);
    }

    [Fact]
    public void WindowSize_HonoursCustomValue()
    {
        var profiler = new DynaCoreProfiler(_bus, 5);
        _bus.Publish(Evt("A", new DynaCoreMetric("temperature", 300d)));

        Assert.Equal(5, profiler.Profiles[("A", "temperature")].WindowSize);
    }

    [Fact]
    public void WindowP50_IsMedianOfWindow()
    {
        var profiler = new DynaCoreProfiler(_bus);

        foreach (var v in new[] { 1d, 2d, 3d, 4d, 5d })
        {
            _bus.Publish(Evt("A", new DynaCoreMetric("x", v)));
        }

        Assert.Equal(3d, profiler.Profiles[("A", "x")].WindowP50);
    }

    [Fact]
    public void WindowPercentiles_AreZeroBeforeAnyEvent()
    {
        var profile = new DynaCoreProfile(DynaCoreProfiler.DefaultWindowSize);

        Assert.Equal(0d, profile.WindowP50);
        Assert.Equal(0d, profile.WindowP95);
        Assert.Equal(0d, profile.WindowP99);
    }
}
