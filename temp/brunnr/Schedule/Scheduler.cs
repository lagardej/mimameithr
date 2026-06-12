using Brunnr.Clock;
using Brunnr.Messaging;
using System.Collections.Concurrent;

namespace Brunnr.Schedule;

/// <summary>
///     Discrete-event scheduler. Maintains a min-heap of jobs keyed by next-due simulated time.
///     Incoming <see cref="JobScheduled" /> messages are queued concurrently and drained into the heap
///     at the start of each <see cref="ClockTicked" />. Due jobs are published as <see cref="JobsDue" />.
///     Each job's original envelope is retained end-to-end so causation can be traced back to the
///     specific <see cref="JobScheduled" /> that put it there, not merged into the batch's envelope.
/// </summary>
public sealed class Scheduler
{
    private readonly IMessageBus _bus;
    private readonly PriorityQueue<Envelope<JobScheduled>, ulong> _heap = new();
    private readonly ConcurrentQueue<Envelope<JobScheduled>> _intake = new();

    public Scheduler(IMessageBus bus)
    {
        _bus = bus;
        bus.Subscribe<JobScheduled>(OnJobScheduled);
        bus.Subscribe<ClockTicked>(OnClockTicked);
    }

    public int PendingJobCount => _heap.Count + _intake.Count;

    public IReadOnlyList<Envelope<JobScheduled>> IntakeQueue => _intake.ToList();

    public IReadOnlyList<(Envelope<JobScheduled> Job, ulong DueAt)> HeapSnapshot =>
        _heap.UnorderedItems.Select(item => (item.Element, item.Priority)).ToList();

    public IReadOnlyList<Envelope<JobScheduled>> JobsDueAt(ulong time) =>
        _heap.UnorderedItems
            .Where(item => item.Priority <= time)
            .Select(item => item.Element)
            .Concat(_intake.Where(item => item.Payload.DueAt <= time))
            .ToList();

    private void OnJobScheduled(Envelope<JobScheduled> envelope) => _intake.Enqueue(envelope);

    private void OnClockTicked(Envelope<ClockTicked> envelope)
    {
        while (_intake.TryDequeue(out var item))
        {
            _heap.Enqueue(item, item.Payload.DueAt);
        }

        var due = new List<Envelope<JobScheduled>>();

        while (_heap.TryPeek(out _, out var dueAt) && dueAt <= envelope.Payload.ElapsedSeconds)
        {
            due.Add(_heap.Dequeue());
        }

        if (due.Count > 0)
        {
            _bus.Publish(envelope.CausedBy(new JobsDue(due, envelope.Payload.ElapsedSeconds)));
        }
    }
}
