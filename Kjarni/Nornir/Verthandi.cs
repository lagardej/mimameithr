using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.Engine.Time;
using Kjarni.Brunnr.System;
using Kjarni.Nornir.Eldr.Irradiance;
using Kjarni.Nornir.Geimr.Position;
using Kjarni.Nornir.Geimr.Rotation;

namespace Kjarni.Nornir;

/// <summary>
///     Headless simulation engine. Drives the <see cref="SystemRoot" /> tick loop.
/// </summary>
internal class Verðandi(EntityStore store)
{
    private readonly MonotonicClock _clock = new();
    private readonly SystemRoot _root = VerðandiSystems.Build(store);

    /// <summary>
    ///     Advances the simulation by <paramref name="deltaTime" /> real seconds, scaled by
    ///     <see cref="TimeCompression" />.
    /// </summary>
    public void Advance(float deltaTime, TimeCompression compression)
    {
        var simDelta = deltaTime * (int) compression;
        _clock.Advance(simDelta);
        _root.Update(new UpdateTick(simDelta, _clock.ElapsedSeconds));
    }

    #region Performance monitor

    /// <summary>Returns a performance log of the last tick, including time spent in each system.</summary>
    /// <returns></returns>
    public string GetPerfLog() => _root.GetPerfLog();

    /// <summary>Enable the performance monitoring.</summary>
    public void EnableMonitorPerf() => _root.SetMonitorPerf(true);

    /// <summary>Disable the performance monitoring.</summary>
    public void DisableMonitorPerf() => _root.SetMonitorPerf(false);

    #endregion
}

/// <summary>
///     Builds the <see cref="SystemRoot" /> for the Kjarni.Nornir simulation.
///     Systems are registered in execution order — dependencies must run before dependents.
/// </summary>
/// <remarks>
///     Entities are bucketed into three update-cadence tiers by physical period (see
///     <see cref="UpdateTiering" />) — a fast rotator/orbiter updates far more often than a slow
///     one, instead of every entity sharing one blanket interval regardless of its own timescale.
///     <see cref="IrradianceSystem" /> operates on cell entities, not the tiered planet-level
///     components, and stays in the medium tier untagged — it isn't currently tiered.
/// </remarks>
internal static class VerðandiSystems
{
    private const float StaggerOffset = 1f;

    /// <summary>
    ///     Creates and returns a configured <see cref="SystemRoot" /> bound to the given <paramref name="store" />.
    /// </summary>
    /// <param name="store">Entity store to bind the system root to.</param>
    public static SystemRoot Build(EntityStore store) => new(store) { FastTier(), MediumTier(), SlowTier() };

    private static StaggeredSystemGroup FastTier() =>
        new("Fast tier", UpdateTiering.FastInterval, StaggerOffset)
        {
            new RotationSystem(Tags.Get<FastTierTag>()), new PositionSystem(Tags.Get<FastTierTag>())
        };

    private static StaggeredSystemGroup MediumTier() =>
        new("Medium tier", UpdateTiering.MediumInterval, StaggerOffset)
        {
            new RotationSystem(Tags.Get<MediumTierTag>()), new PositionSystem(Tags.Get<MediumTierTag>()),
            new IrradianceSystem()
        };

    private static StaggeredSystemGroup SlowTier() =>
        new("Slow tier", UpdateTiering.SlowInterval, StaggerOffset)
        {
            new RotationSystem(Tags.Get<SlowTierTag>()), new PositionSystem(Tags.Get<SlowTierTag>())
        };
}
