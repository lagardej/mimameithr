namespace Brunnr.Clock;

/// <summary>
///     Published by <see cref="Engine" /> after the clock has advanced. Carries the current simulated time since
///     epoch.
/// </summary>
public record ClockTicked(ulong ElapsedSeconds);
