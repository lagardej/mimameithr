using Nornir.Config;
using Nornir.Config.Primosphere.Orbit;
using Nornir.Engine.Clock;
using Nornir.Engine.Messaging;
using Nornir.Engine.System;

namespace Nornir.Component.Primosphere.Motion;

/// <summary>
///     Facade for the motion component.
///     On each tick, schedules one job per domain that has a motion config.
///     Each job delegates to the dynacore; reschedules if the dynacore defers.
/// </summary>
public sealed class Motion(
    IMessageBus bus,
    ConfigRegistry<IDynacoreConfig> configs,
    DynacoreRegistry<IDynacore<IDynacoreConfig>> dynacores)
    : BaseSystem<IDynacoreConfig>(bus, configs, dynacores)
{
    public const string Key = "primosphere-motion";

    /// <inheritdoc />
    protected override SystemId Id { get; } = new(Key);

    /// <inheritdoc />
    protected override void OnTick(Envelope<ClockTicked> envelope)
    {
        foreach (var domainId in Domains())
        {
            ScheduleJob(domainId, envelope.Payload.ElapsedSeconds, envelope, ctx => RunJob(ctx, envelope));
        }
    }

    internal void RunJob(TickContext ctx, Envelope<ClockTicked> cause)
    {
        var domainId = ctx.Domain;
        var (model, config) = Resolve(domainId);
        var dynacore = ResolveDynacore(model);

        var result = dynacore.Compute(config, domainId.Value, CellSet.Global, ctx.ElapsedSeconds);

        if (result.Status == DynacoreStatus.Deferred)
        {
            ScheduleJob(domainId, ctx.ElapsedSeconds + ctx.TickDelta, cause, ctx2 => RunJob(ctx2, cause));
        }
    }
}
