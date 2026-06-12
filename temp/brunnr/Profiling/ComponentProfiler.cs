using Brunnr.Component;
using Brunnr.Messaging;
using System.Collections.Concurrent;

namespace Brunnr.Profiling;

/// <summary>
///     Accumulates per-component timing statistics from <see cref="ComponentTicked" />.
/// </summary>
public sealed class ComponentProfiler
{
    public const int DefaultWindowSize = 1000;

    private readonly ConcurrentDictionary<string, ComponentProfile> _profiles = new();
    private readonly int _windowSize;

    public ComponentProfiler(IMessageBus bus, int windowSize = DefaultWindowSize)
    {
        _windowSize = windowSize;
        bus.Subscribe<ComponentTicked>(OnComponentTicked);
    }

    /// <summary>Per-component timing profiles, keyed by component ID.</summary>
    public IReadOnlyDictionary<string, ComponentProfile> Profiles => _profiles;

    private void OnComponentTicked(Envelope<ComponentTicked> envelope)
    {
        var evt = envelope.Payload;
        var profile = _profiles.GetOrAdd(evt.ComponentId, _ => new ComponentProfile(_windowSize));
        profile.Record(evt.Duration);
    }
}
