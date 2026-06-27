using Brunnr.System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Geodesy;
using Nornir.Aither.BodyRotation;
using Nornir.Aither.Orbit;
using Nornir.Gaea.Tectonics;
using Nornir.Pyr.Irradiance;

namespace Nornir;

/// <summary>
///     Builds the <see cref="SystemRoot" /> for the Nornir simulation.
///     Systems are registered in execution order — dependencies must run before dependents.
/// </summary>
public static class NornirSystems
{
    private const float SimSecond = 1f;
    private const float StaggerOffset = 1f;

    /// <summary>
    ///     Creates and returns a configured <see cref="SystemRoot" /> bound to the given <paramref name="store" />.
    /// </summary>
    /// <param name="store">Entity store to bind the system root to.</param>
    /// <param name="grid">Geodesic grid. Injected until a singleton story is settled.</param>
    /// <param name="seed">World generation seed. Injected until a universe-entity or global seed store exists.</param>
    public static SystemRoot Build(EntityStore store, IGeodesicGrid grid, int seed) =>
        new(store) { StaggeredAt10S(grid, seed) };

    private static StaggeredSystemGroup StaggeredAt10S(IGeodesicGrid grid, int seed) =>
        new("10s staggered systems", SimSecond * 10, StaggerOffset)
        {
            new BodyRotationSystem(), new OrbitSystem(), new IrradianceSystem(), new TectonicsSystem(grid, seed)
        };
}
