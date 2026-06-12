namespace Brunnr.Engine.Messaging;

/// <summary>Helpers for deriving new envelopes from an existing one when reacting to a message.</summary>
public static class EnvelopeExtensions
{
    /// <summary>
    ///     Builds the envelope for a message published in reaction to <paramref name="cause" />.
    ///     Preserves <see cref="Envelope{TPayload}.CorrelationId" />;
    ///     sets <see cref="Envelope{TPayload}.CausationId" /> to the cause's <see cref="Envelope{TPayload}.MessageId" />;
    ///     mints a fresh <see cref="Envelope{TPayload}.MessageId" />.
    /// </summary>
    public static Envelope<TOut> CausedBy<TIn, TOut>(this Envelope<TIn> cause, TOut payload) =>
        new(payload, cause.CorrelationId, Guid.NewGuid(), cause.MessageId);
}
