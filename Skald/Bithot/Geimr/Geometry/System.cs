using Friflo.Engine.ECS;
using Kjarni.Nornir.Geimr.Geometry;

namespace Skald.Bithot.Geimr.Geometry;

/// <summary>
///     Bithot's side of the shared store (see <see cref="Kjarni.Brunnr.Engine.BrunnrEngine" />). Derives
///     render-only components from domain state and keeps them in sync as the domain changes.
/// </summary>
/// <remarks>
///     Forcings mutate domain components mid-tick, not through discrete top-level commands — there is no
///     single call site to hang a "re-derive everything" step on. Instead, this enables
///     <see cref="EntityStore.EventRecorder" /> and uses a query <see cref="EventFilter" /> to detect
///     geometry add/remove events once per render frame. This keeps sync work event-driven and batched.
/// </remarks>
public sealed class GeometrySystem : IBithotSystem
{
    private readonly List<Entity> _pending = [];
    private readonly EntityStore _store;
    private bool _syncAll = true;

    /// <summary>Attaches to <paramref name="store" /> and starts tracking the domain component types it renders.</summary>
    public GeometrySystem(EntityStore store)
    {
        _store = store;
        _store.EventRecorder.Enabled = true;
    }

    /// <summary>Recomputes render components for every entity touched since the last call.</summary>
    public void Advance()
    {
        var eventQuery = _store.Query();
        eventQuery.EventFilter.ComponentAdded<GeometryC>();
        eventQuery.EventFilter.ComponentRemoved<GeometryC>();

        _pending.Clear();
        foreach (var entity in _store.Entities)
            if (_syncAll || eventQuery.HasEvent(entity.Id))
                _pending.Add(entity);

        foreach (var entity in _pending)
        {
            if (entity.HasComponent<GeometryC>())
            {
                var geometry = entity.GetComponent<GeometryC>();
                entity.AddComponent(new VisualRadiusC { Value = VisualScale.ToVisualRadius(geometry.Radius) });
                continue;
            }

            entity.RemoveComponent<VisualRadiusC>();
        }

        _syncAll = false;
        _store.EventRecorder.ClearEvents();
    }
}
