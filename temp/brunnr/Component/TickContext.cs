using Brunnr.Dispatch;
using Brunnr.Domain;
using Brunnr.Messaging;

namespace Brunnr.Component;

/// <summary>
///     Payload passed to a component's tick. Carries the <see cref="JobStarted" /> envelope as the
///     causation parent for anything the tick publishes (e.g. ComponentTicked, JobScheduled).
/// </summary>
public record TickContext(ulong ElapsedSeconds, CellSet CellSet, DomainId Domain, Envelope<JobStarted> Cause);
