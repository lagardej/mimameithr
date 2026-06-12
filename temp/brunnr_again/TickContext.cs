using Brunnr.Engine.Dispatch;
using Brunnr.Engine.Domain;
using Brunnr.Engine.Messaging;

namespace Brunnr.Engine;

public record TickContext(
    ulong ElapsedSeconds,
    ulong TickDelta,
    DomainId Domain,
    CellSet CellSet,
    Envelope<JobStarted> Cause);
