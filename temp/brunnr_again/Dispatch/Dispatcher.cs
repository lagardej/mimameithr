using Brunnr.Engine.Messaging;
using Brunnr.Engine.Schedule;

namespace Brunnr.Engine.Dispatch;

/// <summary>Published by <see cref="Dispatcher" /> immediately before a job is executed.</summary>
public record JobStarted(string ComponentId, ulong ElapsedSeconds);

/// <summary>
///     Executes due jobs in parallel. Subscribes to <see cref="JobsDue" /> and publishes
///     <see cref="JobStarted" /> immediately before invoking each job's tick.
///     Each <see cref="JobStarted" /> is caused by that job's own original <see cref="JobScheduled" />
///     envelope, not by the batched <see cref="JobsDue" /> envelope — preserving per-job causation.
/// </summary>
public sealed class Dispatcher
{
    private readonly IMessageBus _bus;

    public Dispatcher(IMessageBus bus)
    {
        _bus = bus;
        bus.Subscribe<JobsDue>(OnJobsDue);
    }

    private void OnJobsDue(Envelope<JobsDue> envelope) =>
        Parallel.ForEach(envelope.Payload.Jobs, job =>
        {
            var started = job.CausedBy(JobStarted(envelope, job));
            _bus.Publish(started);
            job.Payload.Callback(TickContext(envelope, job, started));
        });

    private static JobStarted JobStarted(Envelope<JobsDue> envelope, Envelope<JobScheduled> job) =>
        new(job.Payload.ComponentId, envelope.Payload.ElapsedSeconds);

    private static TickContext TickContext(Envelope<JobsDue> envelope, Envelope<JobScheduled> job,
        Envelope<JobStarted> started) =>
        new(envelope.Payload.ElapsedSeconds, envelope.Payload.TickDelta, job.Payload.Domain,
            job.Payload.CellSet, started);
}
