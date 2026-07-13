using Friflo.Engine.ECS;
using Nornir.Eldr.Irradiance;

namespace Bithot.Eldr.Irradiance;

/// <summary>
///     Bithot's side of the shared store. Derives <see cref="IrradianceColorC" /> from <see cref="IrradianceC" />
///     every frame.
/// </summary>
/// <remarks>
///     Re-syncs every cell every call rather than diffing against change events, same rationale as
///     <see cref="Geimr.Position.PositionSystem" />/<see cref="Geimr.Rotation.RotationSystem" />: insolation is
///     recomputed every active-phase tick regardless of forcings (rotation, orbit), so there is no cheaper
///     "unchanged" case to detect — unlike <see cref="Geimr.Geometry.GeometrySystem" />'s sparse add/remove
///     events, this has no dirty subset to track.
///     Adding <see cref="IrradianceColorC" /> is a structural change on an entity's first tick (new archetype);
///     recorded via a <see cref="CommandBuffer" /> and played back after the loop rather than applied directly
///     while iterating the live query, which would throw <c>StructuralChangeException</c>.
/// </remarks>
public sealed class IrradianceColorSystem : IBithotSystem
{
    private readonly EntityStore _store;

    /// <summary>Attaches to <paramref name="store" />.</summary>
    public IrradianceColorSystem(EntityStore store)
    {
        _store = store;
    }

    /// <summary>Recomputes every cell's render colour from its current <see cref="IrradianceC" />.</summary>
    public void Advance()
    {
        var buffer = _store.GetCommandBuffer();

        foreach (var entity in _store.Query<IrradianceC>().Entities)
        {
            var irradiance = entity.GetComponent<IrradianceC>();
            buffer.AddComponent(entity.Id,
                new IrradianceColorC { Value = IrradianceColour.FromInsolation(irradiance.Insolation) });
        }

        buffer.Playback();
    }
}
