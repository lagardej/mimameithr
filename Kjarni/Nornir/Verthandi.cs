using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.Engine.Time;

namespace Kjarni.Nornir;

/// <summary>
///     Headless simulation engine. Drives the <see cref="SystemRoot" /> tick loop.
/// </summary>
public class Verðandi(EntityStore store)
{
    private readonly MonotonicClock _clock = new();
    private readonly SystemRoot _root = VerðandiSystems.Build(store);

    /// <summary>Current time compression factor. Defaults to <see cref="TimeCompression.RealTime" />.</summary>
    public TimeCompression Compression { get; set; } = TimeCompression.RealTime;

    /// <summary>Advances the simulation by <paramref name="deltaTime" /> real seconds, scaled by <see cref="Compression" />.</summary>
    public void Advance(float deltaTime)
    {
        var simDelta = deltaTime * (int) Compression;
        _clock.Advance(simDelta);
        _root.Update(new UpdateTick(simDelta, _clock.ElapsedSeconds));
    }

    #region Performance monitor

    /// <summary>Returns a performance log of the last tick, including time spent in each system.</summary>
    /// <returns></returns>
    public string GetPerfLog() => _root.GetPerfLog();

    /// <summary>Enable the performance monitoring.</summary>
    public void EnableMonitorPerf() => _root.SetMonitorPerf(true);

    /// <summary>Disable the performance monitoring.</summary>
    public void DisableMonitorPerf() => _root.SetMonitorPerf(false);

    #endregion
}
