using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Kjarni.Brunnr.System;

/// <summary>
///     A <see cref="SystemGroup" /> that executes its children at a fixed simulated-time interval,
///     with each child staggered by a fixed offset relative to the previous one.
///     Children fire independently — member N fires <paramref name="offset" /> simulated seconds
///     after member N-1. Member order determines firing order.
/// </summary>
public class StaggeredSystemGroup : SystemGroup
{
    private readonly SystemGroup _inner;
    private readonly float _interval;
    private readonly List<(SystemGroup Wrapper, float Accumulator)> _members = [];
    private readonly float _offset;

    public StaggeredSystemGroup(string name, float interval, float offset = 0f) : base(name)
    {
        _interval = interval;
        _offset = offset;
        _inner = new SystemGroup(name + ".Inner");
        base.Add(_inner);
    }

    /// <summary>Adds a system to the staggered group, phased by its insertion order.</summary>
    public new void Add(BaseSystem system)
    {
        var initialAccumulator = -_offset * _members.Count;
        var wrapper = new SystemGroup(system.Name) { system };
        _inner.Add(wrapper);
        wrapper.Enabled = false;
        _members.Add((wrapper, initialAccumulator));
    }

    protected override void OnUpdateGroup()
    {
        var delta = Tick.deltaTime;

        for (var i = 0; i < _members.Count; i++)
        {
            var (wrapper, accumulator) = _members[i];
            accumulator += delta;

            if (accumulator >= _interval)
            {
                wrapper.Enabled = true;
                wrapper.Update(new UpdateTick(accumulator, Tick.time));
                wrapper.Enabled = false;
                accumulator = 0f;
            }

            _members[i] = (wrapper, accumulator);
        }
    }
}
