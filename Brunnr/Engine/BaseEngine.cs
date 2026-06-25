using Brunnr.Engine.Time;
using Brunnr.Grid;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Geodesy;

namespace Brunnr.Engine;

public abstract class BaseEngine
{
    private MonotonicClock _clock = new();
    private SystemRoot? _root;
    private EntityStore? _store;

    /// <summary>Current time compression factor. Defaults to <see cref="TimeCompression.RealTime" />.</summary>
    public TimeCompression Compression { get; set; } = TimeCompression.RealTime;

    protected abstract SystemRoot BuildRoot(EntityStore store);

    /// <summary>
    ///     Initializes the simulation with the given grid and entity setup.
    ///     Replaces any existing configuration.
    ///     Must be called before the first <see cref="Advance" />.
    /// </summary>
    public void Configure(IGeodesicGrid grid, Action<EntityStore> configure)
    {
        GeoGrid.Initialize(grid);
        _store = new EntityStore();
        configure(_store);
        var root = BuildRoot(_store);
        _clock = new MonotonicClock();
        _root = root;
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

    public string GetPerfLog()
    {
        EnsureConfigured();
        return _root!.GetPerfLog();
    }

    public void EnableMonitorPerf()
    {
        EnsureConfigured();
        _root!.SetMonitorPerf(true);
    }

    public void DisableMonitorPerf()
    {
        EnsureConfigured();
        _root!.SetMonitorPerf(false);
    }

    /// <summary>Returns component <typeparamref name="T" /> for the given entity.</summary>
    public T Query<T>(int entityId) where T : struct, IComponent
    {
        EnsureConfigured();
        return _store!.GetEntityById(entityId).GetComponent<T>();
    }
}
