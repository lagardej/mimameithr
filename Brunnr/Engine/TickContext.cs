using Brunnr.Engine.Dispatch;
using Brunnr.Engine.Messaging;

namespace Brunnr.Engine;

public record TickContext(
    ulong ElapsedSeconds,
    ulong TickDelta,
    ulong EntityId,
    Envelope<JobStarted> Cause);
