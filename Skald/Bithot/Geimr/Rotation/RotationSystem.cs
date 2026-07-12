using Bithot.Geimr.Geometry;
using Friflo.Engine.ECS;
using Godot;
using Nornir.Geimr.Rotation;

namespace Bithot.Geimr.Rotation;

/// <summary>
///     Bithot's side of the shared store. Pushes <see cref="RotationC.CurrentAngle" /> onto each body's
///     <see cref="BodyNodeC" /> node every frame, as a rotation around the local Y axis — the same axis
///     <see cref="Geimr.Geometry.RotationAxisGuide" /> draws the rotation-axis guide along.
/// </summary>
/// <remarks>
///     Re-syncs every entity every call, same rationale as <see cref="Position.PositionSystem" />:
///     <see cref="RotationC" /> is recomputed every active-phase tick regardless of forcings, so there is no
///     cheaper "unchanged" case to detect.
/// </remarks>
public sealed class RotationSystem : IBithotSystem
{
    private readonly EntityStore _store;

    /// <summary>Attaches to <paramref name="store" />.</summary>
    public RotationSystem(EntityStore store)
    {
        _store = store;
    }

    /// <summary>Recomputes each body's node rotation from its current <see cref="RotationC" />.</summary>
    public void Advance()
    {
        foreach (var entity in _store.Query<RotationC, BodyNodeC>().Entities)
        {
            var rotation = entity.GetComponent<RotationC>();
            var node = entity.GetComponent<BodyNodeC>().Node;
            node.Rotation = new Vector3(0f, (float)rotation.CurrentAngle.Radians, 0f);

            // GD.Print($"Body_{entity.Id}: rotation angle={rotation.CurrentAngle.Degrees:F1}\u00b0");
        }
    }
}
