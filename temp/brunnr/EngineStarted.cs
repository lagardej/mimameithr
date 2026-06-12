namespace Brunnr;

/// <summary>
///     Published once at the boot -> running transition. Components subscribe to this to perform
///     their first <c>ScheduleAt</c> call, deferred from construction (idle) until the engine actually
///     starts running. Root message — no cause.
/// </summary>
public record EngineStarted;
