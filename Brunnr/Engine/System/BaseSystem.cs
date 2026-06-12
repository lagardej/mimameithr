using Brunnr.Engine.Clock;
using Brunnr.Engine.Messaging;
using Brunnr.Engine.Schedule;

namespace Brunnr.Engine.System;

public readonly record struct SystemId(string Value);

public abstract class BaseSystem(IMessageBus bus) : ISubscriber
{
    protected abstract SystemId Id { get; }

    public void Subscribe() => bus.Subscribe<ClockTicked>(OnTick);

    protected abstract void OnTick(Envelope<ClockTicked> envelope);

    protected void ScheduleJob(ulong entityId, ulong dueAt, Envelope<ClockTicked> cause,
        Action<TickContext> callback) =>
        bus.Publish(new Envelope<JobScheduled>(
            new JobScheduled(Id, entityId, dueAt, callback),
            cause.CorrelationId,
            Guid.NewGuid(),
            cause.MessageId));
}
