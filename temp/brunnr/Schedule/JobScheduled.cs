using Brunnr.Component;
using Brunnr.Domain;

namespace Brunnr.Schedule;

/// <summary>Published by a component to register a one-shot job with the <see cref="Scheduler" />.</summary>
public record JobScheduled(string ComponentId, DomainId Domain, ulong DueAt, Action<TickContext> Tick, CellSet CellSet);
