using Brunnr.Engine.Clock;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Science.Geography.Spatial;

namespace Nornir;

/// <summary>
///     Headless simulation engine. Owns the <see cref="EntityStore" /> and drives the
///     <see cref="SystemRoot" /> tick loop.
/// </summary>
public sealed class Nornir
{
    private readonly MonotonicClock _clock = new();
    private readonly SystemRoot _root;
    private readonly EntityStore _store;

    public Nornir(IGeoGrid grid)
    {
        Grid.Initialize(grid);
        _store = new EntityStore();
        _root = NornirSystems.Build(_store);
        _root.SetMonitorPerf(true);
    }

    /// <summary>Current time compression factor. Defaults to <see cref="TimeCompression.RealTime" />.</summary>
    public TimeCompression Compression { get; set; } = TimeCompression.RealTime;

    /// <summary>
    ///     Provides access to the <see cref="EntityStore" /> for world setup.
    ///     Must be called before the first <see cref="Advance" />.
    /// </summary>
    public void Configure(Action<EntityStore> configure) => configure(_store);

    /// <summary>Advances the simulation by <paramref name="deltaTime" /> real seconds, scaled by <see cref="Compression" />.</summary>
    public void Advance(float deltaTime)
    {
        var simDelta = deltaTime * (int) Compression;
        _clock.Advance(simDelta);
        _root.Update(new UpdateTick(simDelta, _clock.ElapsedSeconds));
    }

    /// <summary>Returns component <typeparamref name="T" /> for the given entity.</summary>
    public T Query<T>(int entityId) where T : struct, IComponent =>
        _store.GetEntityById(entityId).GetComponent<T>();

    public string GetPerfLog() => _root.GetPerfLog();
}
