namespace Brunnr.Messaging;

/// <summary>
///     Pub/sub message bus. Publishers and subscribers are decoupled; message types are defined by callers.
///     Transports <see cref="Envelope{TPayload}" />-wrapped messages as-is — it does not mint or infer
///     correlation/causation metadata; that is the publisher's responsibility.
/// </summary>
public interface IMessageBus
{
    /// <summary>Publish <paramref name="envelope" /> to all current subscribers of <typeparamref name="TPayload" />.</summary>
    void Publish<TPayload>(Envelope<TPayload> envelope);

    /// <summary>
    ///     Subscribe to messages with payload type <typeparamref name="TPayload" />.
    ///     Returns a token; dispose it to unsubscribe.
    /// </summary>
    IDisposable Subscribe<TPayload>(Action<Envelope<TPayload>> handler);
}
