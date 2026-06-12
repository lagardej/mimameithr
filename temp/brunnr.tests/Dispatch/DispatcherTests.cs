using Brunnr.Component;
using Brunnr.Dispatch;
using Brunnr.Domain;
using Brunnr.Messaging;
using Brunnr.Schedule;
using Xunit;

namespace Brunnr.Tests.Dispatch;

public sealed class DispatcherTests
{
    private readonly InMemoryMessageBus _bus = new();

    private static Envelope<JobScheduled> MakeJob(Action<TickContext>? tick = null)
    {
        var job = new JobScheduled(
            "TestComponent",
            new DomainId("test"),
            0,
            tick ?? (_ => { }),
            CellSet.Global
        );
        return Envelope<JobScheduled>.Root(job);
    }

    private void PublishJobsDue(IReadOnlyList<Envelope<JobScheduled>> jobs, ulong elapsedSeconds) =>
        _bus.Publish(Envelope<JobsDue>.Root(new JobsDue(jobs, elapsedSeconds)));

    [Fact]
    public void JobStarted_PublishedBeforeTick()
    {
        _ = new Dispatcher(_bus);
        var order = new List<string>();

        var job = MakeJob(_ => order.Add("tick"));
        _bus.Subscribe<JobStarted>(_ => order.Add("started"));

        PublishJobsDue([job], 1);

        Assert.Equal(["started", "tick"], order);
    }

    [Fact]
    public void JobStarted_CarriesCorrectComponentIdAndTime()
    {
        _ = new Dispatcher(_bus);
        Envelope<JobStarted>? received = null;
        _bus.Subscribe<JobStarted>(evt => received = evt);

        PublishJobsDue([MakeJob()], 42);

        Assert.NotNull(received);
        Assert.Equal("TestComponent", received.Payload.ComponentId);
        Assert.Equal(42ul, received.Payload.ElapsedSeconds);
    }

    [Fact]
    public void AllJobsExecuted()
    {
        _ = new Dispatcher(_bus);
        var executed = 0;

        var jobs = Enumerable.Range(0, 5).Select(_ => MakeJob(_ => Interlocked.Increment(ref executed))).ToList();

        PublishJobsDue(jobs, 1);

        Assert.Equal(5, executed);
    }

    [Fact]
    public void TickContext_CarriesCorrectElapsedSeconds()
    {
        _ = new Dispatcher(_bus);
        ulong? received = null;

        var job = MakeJob(ctx => received = ctx.ElapsedSeconds);

        PublishJobsDue([job], 99);

        Assert.Equal(99ul, received);
    }

    [Fact]
    public void NoJobs_NothingPublished()
    {
        _ = new Dispatcher(_bus);
        var count = 0;
        _bus.Subscribe<JobStarted>(_ => count++);

        PublishJobsDue([], 1);

        Assert.Equal(0, count);
    }
}
