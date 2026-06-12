namespace Brunnr.Engine.Messaging;

/// <summary>
///     Wraps a message payload with transport metadata for cross-cascade tracing.
///     The bus carries envelopes as-is; it does not mint or infer correlation/causation —
///     publishers are responsible for constructing the envelope they hand to the bus.
/// </summary>
/// <param name="Payload">The actual message content.</param>
/// <param name="CorrelationId">
///     Shared by every envelope in a single causal cascade (e.g. one clock tick and everything it triggers).
///     Constant across the whole chain — copy it forward when reacting to an envelope.
/// </param>
/// <param name="MessageId">
///     This envelope's own identity. Becomes the <see cref="CausationId" /> of anything published in
///     reaction to it.
/// </param>
/// <param name="CausationId">
///     The <see cref="MessageId" /> of the envelope whose handler produced this one.
///     <c>null</c> for root envelopes (no prior cause — e.g. an external call into the engine).
/// </param>
public sealed record Envelope<TPayload>(
    TPayload Payload,
    Guid CorrelationId,
    Guid MessageId,
    Guid? CausationId
)
{
    /// <summary>Mints a fresh envelope with no cause — for publishes not in reaction to any prior message.</summary>
    public static Envelope<TPayload> Root(TPayload payload) =>
        new(payload, Guid.NewGuid(), Guid.NewGuid(), null);
}
