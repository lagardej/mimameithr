using Brunnr.Engine.Clock;
using Brunnr.Engine.Config;
using Brunnr.Engine.Domain;
using Brunnr.Engine.Messaging;
using Brunnr.Engine.Schedule;

namespace Brunnr.Engine.System;

public readonly record struct SystemId(string Value);

public abstract class BaseSystem<TConfig>(IMessageBus bus, ConfigRegistry<TConfig> configs) where TConfig: IConfig
{
    protected abstract SystemId Id { get; }

    public void Subscribe() => bus.Subscribe<ClockTicked>(OnTick);

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
