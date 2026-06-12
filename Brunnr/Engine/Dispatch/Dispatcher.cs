using Brunnr.Engine.Messaging;
using Brunnr.Engine.Schedule;
using Brunnr.Engine.System;

namespace Brunnr.Engine.Dispatch;

/// <summary>Published by <see cref="Dispatcher" /> immediately before a job is executed.</summary>
public record JobStarted(SystemId SystemId, ulong ElapsedSeconds);

/// <summary>
///     Executes due jobs in parallel. Subscribes to <see cref="JobsDue" /> and publishes
///     <see cref="JobStarted" /> immediately before invoking each job's tick.
///     Each <see cref="JobStarted" /> is caused by that job's own original <see cref="JobScheduled" />
///     envelope, not by the batched <see cref="JobsDue" /> envelope — preserving per-job causation.
/// </summary>
public sealed class Dispatcher(IMessageBus bus) : ISubscriber
{
    public void Subscribe() => bus.Subscribe<JobsDue>(OnJobsDue);

    private void OnJobsDue(Envelope<JobsDue> envelope) =>
        Parallel.ForEach(envelope.Payload.Jobs, job =>
        {
            var started = job.CausedBy(new JobStarted(job.Payload.SystemId, envelope.Payload.ElapsedSeconds));
            bus.Publish(started);
            job.Payload.Callback(new TickContext(
                envelope.Payload.ElapsedSeconds,
                envelope.Payload.TickDelta,
                job.Payload.EntityId,
                started));
        });
}
