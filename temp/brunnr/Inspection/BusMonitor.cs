using Brunnr.Messaging;
using System.Collections.Concurrent;

namespace Brunnr.Inspection;

/// <summary>
///     Wraps <see cref="IMessageBus" /> and counts publishes per message payload type.
///     Pass this as the bus to all engine components; consumers subscribe to the same instance.
/// </summary>
public sealed class BusMonitor(IMessageBus inner) : IMessageBus
{
    private readonly ConcurrentDictionary<string, long> _counts = new();

    /// <summary>Publish counts keyed by message payload type name.</summary>
    public IReadOnlyDictionary<string, long> Counts => _counts;

    public void Publish<TPayload>(Envelope<TPayload> envelope)
    {
        _counts.AddOrUpdate(typeof(TPayload).Name, 1, (_, n) => n + 1);
        inner.Publish(envelope);
    }

    public IDisposable Subscribe<TPayload>(Action<Envelope<TPayload>> handler) =>
        inner.Subscribe(handler);
}
