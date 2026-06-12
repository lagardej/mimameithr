using Brunnr.Component;
using Brunnr.Messaging;
using System.Collections.Concurrent;

namespace Brunnr.Profiling;

/// <summary>
///     Accumulates per-metric DynaCore statistics from <see cref="ComponentTicked" />.
/// </summary>
public sealed class DynaCoreProfiler
{
    public const int DefaultWindowSize = 1000;

    private readonly ConcurrentDictionary<(string ComponentId, string MetricName), DynaCoreProfile> _profiles = new();
    private readonly int _windowSize;

    public DynaCoreProfiler(IMessageBus bus, int windowSize = DefaultWindowSize)
    {
        _windowSize = windowSize;
        bus.Subscribe<ComponentTicked>(OnComponentTicked);
    }

    /// <summary>Per-metric DynaCore profiles, keyed by (component ID, metric name).</summary>
    public IReadOnlyDictionary<(string ComponentId, string MetricName), DynaCoreProfile> Profiles => _profiles;

    private void OnComponentTicked(Envelope<ComponentTicked> envelope)
    {
        var evt = envelope.Payload;
        foreach (var metric in evt.Metrics)
        {
            var key = (evt.ComponentId, metric.Name);
            var profile = _profiles.GetOrAdd(key, _ => new DynaCoreProfile(_windowSize));
            profile.Record(metric.Value);
        }
    }
}
