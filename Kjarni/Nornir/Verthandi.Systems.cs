using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.System;
using Kjarni.Nornir.Eldr.Irradiance;
using Kjarni.Nornir.Geimr.Orbit;
using Kjarni.Nornir.Geimr.Rotation;

namespace Kjarni.Nornir;

/// <summary>
///     Builds the <see cref="SystemRoot" /> for the Kjarni.Nornir simulation.
///     Systems are registered in execution order — dependencies must run before dependents.
/// </summary>
public static class VerðandiSystems
{
    private const float SimSecond = 1f;
    private const float StaggerOffset = 1f;

    /// <summary>
    ///     Creates and returns a configured <see cref="SystemRoot" /> bound to the given <paramref name="store" />.
    /// </summary>
    /// <param name="store">Entity store to bind the system root to.</param>
    public static SystemRoot Build(EntityStore store) => new(store) { StaggeredAt10S() };

    private static StaggeredSystemGroup StaggeredAt10S() =>
        new("10s staggered systems", SimSecond * 10, StaggerOffset)
        {
            new RotationSystem(), new OrbitSystem(), new IrradianceSystem()
        };
}
