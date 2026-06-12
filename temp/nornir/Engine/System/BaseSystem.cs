using Nornir.Config;
using Nornir.Engine.Clock;
using Nornir.Engine.Domain;
using Nornir.Engine.Messaging;
using Nornir.Engine.Schedule;

namespace Nornir.Engine.System;

public readonly record struct SystemId(string Value);

public abstract class BaseSystem<TConfig>(IMessageBus bus, ConfigRegistry<TConfig> configs)
{
    protected abstract SystemId Id { get; }

    public BaseSystem<TConfig> Subscribe()
    {
        bus.Subscribe<ClockTicked>(OnTick);
        return this;
    }

    protected abstract void OnTick(Envelope<ClockTicked> envelope);

    protected void ScheduleJob(DomainId domainId, ulong dueAt, Envelope<ClockTicked> cause,
        Action<TickContext> callback)
    {
        var jobScheduled = new JobScheduled(
            Id.Value,
            domainId,
            dueAt,
            callback,
            CellSet.Global);

        bus.Publish(new Envelope<JobScheduled>(
            jobScheduled,
            cause.CorrelationId,
            Guid.NewGuid(),
            cause.MessageId));
    }

    protected IEnumerable<DomainId> Domains() => configs.Domains();

    protected TConfig Resolve(DomainId domainId) => configs.Resolve(domainId);
}
