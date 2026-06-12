using Brunnr.Engine.Messaging;

namespace Brunnr.Engine.Schedule;

/// <summary>
///     Published by <see cref="Scheduler" /> when one or more jobs are due for the current tick.
///     Each job retains its own original <see cref="Envelope{TPayload}" /> so dispatch can derive
///     <c>JobStarted</c> with causation pointing back to that specific scheduling, not to this batch.
/// </summary>
public record JobsDue(IReadOnlyList<Envelope<JobScheduled>> Jobs, ulong ElapsedSeconds, ulong TickDelta);
