using Friflo.Engine.ECS;
using Godot;
using Kjarni.Nornir.Geimr.Position;
using Skald.Bithot.Geimr.Geometry;

namespace Skald.Bithot.Geimr.Position;

/// <summary>
///     Bithot's side of the shared store. Pushes <see cref="PositionC" />, scaled via <see cref="PositionScale" />,
///     onto each body's <see cref="BodyNodeC" /> node every frame.
/// </summary>
/// <remarks>
///     Re-syncs every entity every call rather than diffing against change events: <see cref="PositionC" /> is
///     recomputed from orbital mechanics every active-phase tick regardless of forcings, so there is no cheaper
///     "unchanged" case to detect here — full resync is the correct behaviour, not a simplification.
/// </remarks>
public sealed class PositionSystem : IBithotSystem
{
    private readonly EntityStore _store;

    /// <summary>Attaches to <paramref name="store" />.</summary>
    public PositionSystem(EntityStore store)
    {
        _store = store;
    }

    /// <summary>Recomputes each body's node position from its current <see cref="PositionC" />.</summary>
    public void Advance()
    {
        foreach (var entity in _store.Query<PositionC, BodyNodeC>().Entities)
        {
            var position = entity.GetComponent<PositionC>();
            var node = entity.GetComponent<BodyNodeC>().Node;
            var visual = PositionScale.ToVisualPosition(position.X, position.Y, position.Z);
            node.Position = visual;

            // GD.Print($"Body_{entity.Id}: real pos=({position.X.Kilometers:N0}, {position.Y.Kilometers:N0}, " + $"{position.Z.Kilometers:N0}) km, visual pos={visual}");
        }
    }
}
