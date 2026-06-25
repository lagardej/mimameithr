namespace Brunnr.Engine.Time;

/// <summary>
///     Simulated-time multiplier applied to each real-time delta before advancing the simulation.
/// </summary>
public enum TimeCompression
{
    /// <summary>1 real second = 1 simulated second. Avatar mode baseline.</summary>
    RealTime = 1,

    /// <summary>1 real second = 1 simulated minute (×60).</summary>
    Minute = 60,

    /// <summary>1 real second = 1 simulated hour (×3 600).</summary>
    Hour = 3_600,

    /// <summary>1 real second = 1 simulated day (×86 400).</summary>
    Day = 86_400
}
