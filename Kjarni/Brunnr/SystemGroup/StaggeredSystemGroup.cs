using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Brunnr.SystemGroup;

/// <summary>
///     A <see cref="SystemGroup" /> that executes its children at a fixed simulated-time interval,
///     with each child staggered by a fixed offset relative to the previous one.
///     Children fire independently — each child fires after the previous one by the specified offset.
///     Member order determines firing order.
/// </summary>
public class StaggeredSystemGroup : Friflo.Engine.ECS.Systems.SystemGroup
{
    private readonly Friflo.Engine.ECS.Systems.SystemGroup _inner;
    private readonly float _interval;

    private readonly List<(Friflo.Engine.ECS.Systems.SystemGroup Wrapper, float Accumulator, float Threshold)>
        _members = [];

    private readonly float _offset;

    /// <summary>
    ///     Initializes a new staggered system group with the specified interval and offset.
    /// </summary>
    /// <param name="name">Name of the system group.</param>
    /// <param name="interval">Fixed simulated-time interval between system executions, in seconds.</param>
    /// <param name="offset">Initial phase offset for staggered execution, in seconds.</param>
    public StaggeredSystemGroup(string name, float interval, float offset = 0f) : base(name)
    {
        _interval = interval;
        _offset = offset;
        _inner = new Friflo.Engine.ECS.Systems.SystemGroup(name + ".Inner");
        base.Add(_inner);
    }

    /// <summary>Adds a system to the staggered group, phased by its insertion order.</summary>
    /// <remarks>
    ///     The member's first fire happens after <c>offset * insertion index</c> seconds — quickly,
    ///     not after a full <see cref="_interval" /> — so entities get an initial update shortly after
    ///     boot instead of sitting on stale generation-phase state for up to one interval. Every fire
    ///     after the first uses the full <see cref="_interval" />.
    /// </remarks>
    public new void Add(BaseSystem system)
    {
        var firstFireThreshold = _offset * _members.Count;
        var wrapper = new Friflo.Engine.ECS.Systems.SystemGroup(system.Name) { system };
        _inner.Add(wrapper);
        wrapper.Enabled = false;
        _members.Add((wrapper, 0f, firstFireThreshold));
    }

    /// <summary>Updates the staggered group by accumulating time and executing systems on interval.</summary>
    protected override void OnUpdateGroup()
    {
        var delta = Tick.deltaTime;

        for (var i = 0; i < _members.Count; i++)
        {
            var (wrapper, accumulator, threshold) = _members[i];
            accumulator += delta;

            if (accumulator >= threshold)
            {
                wrapper.Enabled = true;
                wrapper.Update(new UpdateTick(accumulator, Tick.time));
                wrapper.Enabled = false;
                accumulator = 0f;
                threshold = _interval;
            }

            _members[i] = (wrapper, accumulator, threshold);
        }
    }
}
