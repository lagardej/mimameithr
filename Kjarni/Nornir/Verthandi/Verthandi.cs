using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.Engine.Time;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Natural.Physical.Geodesy;

namespace Kjarni.Nornir.Verthandi;

/// <summary>
///     Headless simulation engine. Drives the <see cref="SystemRoot" /> tick loop.
/// </summary>
public class Verðandi(EntityStore store)
{
    private MonotonicClock _clock = new();
    private SystemRoot? _root;

    /// <summary>Current time compression factor. Defaults to <see cref="TimeCompression.RealTime" />.</summary>
    public TimeCompression Compression { get; set; } = TimeCompression.RealTime;

    /// <summary>
    ///     Builds the system root against the entity store passed at construction.
    ///     Replaces any existing configuration.
    ///     Must be called before the first <see cref="Advance" />.
    /// </summary>
    public void Configure(IGeodesicGrid grid)
    {
        GeoGrid.Initialize(grid);
        _root = VerðandiSystems.Build(store);
        _clock = new MonotonicClock();
    }

    private void EnsureConfigured()
    {
        if (_root is null)
        {
            throw new InvalidOperationException("Engine is not configured. Call Configure first.");
        }
    }

    /// <summary>Advances the simulation by <paramref name="deltaTime" /> real seconds, scaled by <see cref="Compression" />.</summary>
    public void Advance(float deltaTime)
    {
        EnsureConfigured();
        var simDelta = deltaTime * (int) Compression;
        _clock.Advance(simDelta);
        _root!.Update(new UpdateTick(simDelta, _clock.ElapsedSeconds));
    }

    /// <summary>Returns component <typeparamref name="T" /> for the given entity.</summary>
    public T Query<T>(int entityId) where T : struct, IComponent
    {
        EnsureConfigured();
        return store.GetEntityById(entityId).GetComponent<T>();
    }

    #region Performance monitor

    /// <summary>Returns a performance log of the last tick, including time spent in each system.</summary>
    /// <returns></returns>
    public string GetPerfLog()
    {
        EnsureConfigured();
        return _root!.GetPerfLog();
    }

    /// <summary>Enable the performance monitoring.</summary>
    public void EnableMonitorPerf()
    {
        EnsureConfigured();
        _root!.SetMonitorPerf(true);
    }

    /// <summary>Disable the performance monitoring.</summary>
    public void DisableMonitorPerf()
    {
        EnsureConfigured();
        _root!.SetMonitorPerf(false);
    }

    #endregion
}
