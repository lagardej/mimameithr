using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Nornir;

/// <summary>
///     A <see cref="SystemGroup" /> that executes its children at a fixed simulated-time interval,
///     with each child staggered by a fixed offset relative to the previous one.
///     Children fire independently — member N fires <paramref name="offset" /> simulated seconds
///     after member N-1. Member order determines firing order.
/// </summary>
public class StaggeredSystemGroup(string name, float interval, float offset) : SystemGroup(name)
{
    private readonly List<(SystemGroup Wrapper, float Accumulator)> _members = [];

    /// <summary>Adds a system to the staggered group, phased by its insertion order.</summary>
    public new void Add(BaseSystem system)
    {
        var initialAccumulator = -offset * _members.Count;
        var wrapper = new SystemGroup(system.Name) { system };
        _members.Add((wrapper, initialAccumulator));
    }

    protected override void OnUpdateGroup()
    {
        var delta = Tick.deltaTime;

        for (var i = 0; i < _members.Count; i++)
        {
            var (wrapper, accumulator) = _members[i];
            accumulator += delta;

            if (accumulator >= interval)
            {
                wrapper.Update(new UpdateTick(accumulator, Tick.time));
                accumulator = 0f;
            }

            _members[i] = (wrapper, accumulator);
        }
    }
}
