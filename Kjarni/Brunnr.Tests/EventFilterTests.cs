using Friflo.Engine.ECS;
using Xunit;

namespace Kjarni.Brunnr.Tests;

/// <summary>Test-only component; isolates the check from any domain component.</summary>
public struct ProbeC : IComponent
{
    public int Value;
}

/// <summary>
///     Resolves the open question in <c>geometrysystem-missed-updates.md</c>: does
///     <see cref="EventFilter.ComponentAdded{T}" /> fire when <c>AddComponent</c> is called on an entity that
///     already has the component (an <c>Update</c>, not a genuine first-attach)?
/// </summary>
public class EventFilterTests
{
    [Fact]
    public void ComponentAdded_FiresOnFirstAttach()
    {
        var store = new EntityStore();
        store.EventRecorder.Enabled = true;

        var entity = store.CreateEntity();
        entity.AddComponent(new ProbeC { Value = 1 });

        var query = store.Query();
        query.EventFilter.ComponentAdded<ProbeC>();

        Assert.True(query.HasEvent(entity.Id));
    }

    [Fact]
    public void ComponentAdded_FiresOnAddOverExisting()
    {
        var store = new EntityStore();
        store.EventRecorder.Enabled = true;

        var entity = store.CreateEntity();
        entity.AddComponent(new ProbeC { Value = 1 });

        // Clear the first-attach event so only the second call is under test.
        store.EventRecorder.ClearEvents();

        var query = store.Query();
        query.EventFilter.ComponentAdded<ProbeC>();

        // Component already present — this is an Update, not a first-attach.
        entity.AddComponent(new ProbeC { Value = 2 });

        // If this passes: hypothesis (2) holds — AddComponent fires ComponentAdded structurally
        // regardless of prior presence, GeometrySystem already catches forcing-driven overwrites,
        // no gap, no fix needed.
        // If this fails: hypothesis (1) holds — EventFilter is blind to Add-over-existing, gap is
        // real, switch GeometrySystem to store.OnComponentChanged per the Fix section.
        Assert.True(query.HasEvent(entity.Id));
    }
}
