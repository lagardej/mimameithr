using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Kjarni.Brunnr.System;

/// <summary>
///     A <see cref="SystemGroup" /> that executes its children at a fixed simulated-time interval.
///     On each engine tick the elapsed delta is accumulated.
///     When the accumulator reaches <see cref="Interval" />, children are dispatched once with a
///     <see cref="UpdateTick" /> whose <c>deltaTime</c> equals the accumulated time, then the
///     accumulator resets. Ticks where the accumulator has not yet reached the interval are no-ops.
/// </summary>
public class ThrottledSystemGroup : SystemGroup
{
    private readonly SystemGroup _inner;
    private float _accumulator;

    public ThrottledSystemGroup(string name, float interval, float offset = 0f) : base(name)
    {
        Interval = interval;
        _accumulator = -offset;
        _inner = new SystemGroup(name + ".Inner");
        base.Add(_inner);
    }

    /// <summary>Simulated-time interval between executions, in seconds.</summary>
    private float Interval { get; }

    /// <summary>Adds a system to the throttled group.</summary>
    public new void Add(BaseSystem system) => _inner.Add(system);

    protected override void OnUpdateGroup()
    {
        _accumulator += Tick.deltaTime;

        if (_accumulator < Interval)
        {
            return;
        }

        _inner.Update(new UpdateTick(_accumulator, Tick.time));
        _accumulator = 0f;
    }
}
