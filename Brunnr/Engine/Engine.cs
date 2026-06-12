using Brunnr.Engine.Clock;
using Brunnr.Engine.Messaging;

namespace Brunnr.Engine;

public sealed class Engine(IMessageBus bus, ulong tickDuration = 1)
{
    private readonly MonotonicClock _clock = new();

    public void Tick()
    {
        _clock.Advance(tickDuration);
        bus.Publish(Envelope<ClockTicked>.Root(new ClockTicked(_clock.ElapsedSeconds, tickDuration)));
    }
}
