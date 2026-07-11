using Friflo.Engine.ECS;

using Skald.Bithot.Geimr.Geometry;
using Skald.Bithot.Geimr.Position;
using Skald.Bithot.Geimr.Rotation;

namespace Skald.Bithot;

/// <summary>Builds the ordered set of Bithot systems attached to a shared <see cref="EntityStore" />.</summary>
internal static class BithotSystemRegistry
{
    /// <summary>Creates systems in deterministic execution order.</summary>
    internal static IReadOnlyList<IBithotSystem> Build(EntityStore store)
    {
        return
        [
            new GeometrySystem(store),
            new PositionSystem(store),
            new RotationSystem(store)
        ];
    }
}
