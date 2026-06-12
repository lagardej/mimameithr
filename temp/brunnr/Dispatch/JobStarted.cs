namespace Brunnr.Dispatch;

/// <summary>Published by <see cref="Dispatcher" /> immediately before a job is executed.</summary>
public record JobStarted(string ComponentId, ulong ElapsedSeconds);
