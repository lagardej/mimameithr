using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Xunit;

namespace Kjarni.Brunnr.Tests;

/// <summary>Test-only component to give <see cref="RecordingSystem" /> a query type to bind to.</summary>
public struct StaggerProbeC : IComponent
{
    public int Value;
}

/// <summary>Records the cumulative real-time elapsed at each <see cref="OnUpdate" /> call.</summary>
public sealed class RecordingSystem(List<float> fireTimes) : QuerySystem<StaggerProbeC>
{
    protected override void OnUpdate() => fireTimes.Add(Tick.time);
}

/// <summary>
///     Regression coverage: a member's first fire should happen quickly (after roughly
///     <c>offset * index</c> seconds), not after a full <c>interval</c> — entities shouldn't sit on
///     stale generation-phase state for up to one interval after boot. See kanban
///     <c>adaptive-tick-scheduling.md</c>.
/// </summary>
public class StaggeredSystemGroupTests
{
    [Fact]
    public void Members_FireQuickly_ThenSettleIntoInterval()
    {
        var store = new EntityStore();
        store.CreateEntity(new StaggerProbeC());
        var fireTimesA = new List<float>();
        var fireTimesB = new List<float>();
        var fireTimesC = new List<float>();

        var group = new System.StaggeredSystemGroup("test", interval: 10f, offset: 1f)
        {
            new RecordingSystem(fireTimesA), new RecordingSystem(fireTimesB), new RecordingSystem(fireTimesC)
        };
        var root = new SystemRoot(store) { group };

        // Tick granularity (0.5s) must be finer than the offset (1s) or successive members'
        // thresholds land on the same tick and the stagger can't be observed.
        var elapsed = 0f;
        for (var i = 0; i < 80; i++)
        {
            elapsed += 0.5f;
            root.Update(new UpdateTick(0.5f, elapsed));
        }

        // First fires happen close to offset*index (0.5, 1.0, 2.0 — rounded up to the next tick
        // boundary) — not interval+offset*index (10, 11, 12).
        Assert.Equal(0.5f, fireTimesA[0]);
        Assert.Equal(1.0f, fireTimesB[0]);
        Assert.Equal(2.0f, fireTimesC[0]);

        // Subsequent fires settle into the full interval from each member's own first fire.
        Assert.Equal(10.5f, fireTimesA[1]);
        Assert.Equal(11.0f, fireTimesB[1]);
        Assert.Equal(12.0f, fireTimesC[1]);
        Assert.Equal(20.5f, fireTimesA[2]);
        Assert.Equal(21.0f, fireTimesB[2]);
        Assert.Equal(22.0f, fireTimesC[2]);
    }
}
