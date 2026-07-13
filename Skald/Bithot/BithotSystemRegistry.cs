using Bithot.Eldr.Irradiance;
using Bithot.Geimr.Geometry;
using Bithot.Geimr.Position;
using Bithot.Geimr.Rotation;
using Friflo.Engine.ECS;

namespace Bithot;

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
            new RotationSystem(store),
            new IrradianceColorSystem(store)
        ];
    }
}