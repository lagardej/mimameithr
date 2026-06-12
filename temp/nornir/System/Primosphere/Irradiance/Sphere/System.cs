using Nornir.Config;
using Nornir.Engine.Clock;
using Nornir.Engine.Domain;
using Nornir.Engine.Messaging;
using Nornir.Engine.System;

namespace Nornir.System.Primosphere.Irradiance.Sphere;

public sealed class IrradianceSphereSystem(
    IMessageBus bus,
    ConfigRegistry configs,
    IStoreReader storeReader,
    IStoreWriter storeWriter) : BaseSystem(bus, configs)
{
    public const string Key = "primosphere-irradiance-sphere";

    protected override SystemId Id { get; } = new(Key);

    protected override void OnTick(Envelope<ClockTicked> envelope)
    {
        foreach (var domainId in Domains())
        {
            ScheduleJob(domainId, envelope.Payload.ElapsedSeconds, envelope, ctx => RunJob(ctx, envelope));
        }
    }

    private void RunJob(TickContext ctx, Envelope<ClockTicked> cause)
    {
        var domainId = ctx.Domain;
        var config = Resolve(domainId);

        var result = dynacore.Compute(config, domainId.Value, CellSet.Global, ctx.ElapsedSeconds);

        if (result.Status == DynacoreStatus.Deferred)
        {
            ScheduleJob(domainId, ctx.ElapsedSeconds + ctx.TickDelta, cause, ctx2 => RunJob(ctx2, cause));
        }
    }
}
