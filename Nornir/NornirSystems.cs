using Brunnr.System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Nornir.Element.Aither.BodyRotation;
using Nornir.Element.Aither.Orbit;
using Nornir.Element.Pyr.Irradiance;

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
    public static SystemRoot Build(EntityStore store) =>
        new(store) { StaggeredAt10S() };

    private static StaggeredSystemGroup StaggeredAt10S() =>
        new("10s staggered systems", SimSecond * 10, StaggerOffset)
        {
            new BodyRotationSystem(), new OrbitSystem(), new IrradianceSystem()
        };
}
