using Brunnr.Clock;
using Brunnr.Component;
using Brunnr.Messaging;
using Brunnr.Schedule;

namespace Brunnr;

/// <summary>
///     Engine entry point. Call <see cref="Configure(EngineConfiguration)" /> to set
///     domain/component configuration, <see cref="Start" /> once after all components are constructed
///     (boot complete) to transition into the running phase, then <see cref="Tick" /> at the desired
///     rate to drive the simulation. One call per second = real-time; N calls per second = N× time compression.
///     On <see cref="Start" />, the bootstrapper publishes <see cref="EngineStarted" /> and orchestrates
///     initial <see cref="JobScheduled" /> events for each component-domain pair based on configuration.
/// </summary>
public sealed class Engine(
    IMessageBus bus,
    IComponentRegistry componentRegistry,
    ulong tickDuration = 1)
{
    private readonly Clock.Clock _clock = new();
    private EngineConfiguration _configuration = new();

    /// <summary>Sets bootstrap configuration used when <see cref="Start" /> is called.</summary>
    public void Configure(EngineConfiguration configuration) =>
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    ///     Transitions the engine from boot (idle) to running.
    ///     Publishes <see cref="EngineStarted" />, then bootstraps component scheduling for all configured domains.
    /// </summary>
    public void Start()
    {
        var engineStartedEnvelope = Envelope<EngineStarted>.Root(new EngineStarted());
        bus.Publish(engineStartedEnvelope);

        // Bootstrap: publish JobScheduled for each component-domain pair
        foreach (var domainConfig in _configuration.Domains)
        {
            foreach (var (componentName, _) in domainConfig.Bindings)
            {
                var component = componentRegistry.Get(componentName);
                component?.ScheduleFor(domainConfig.Id, 0, engineStartedEnvelope, CellSet.Global);
            }
        }
    }

    public void Tick()
    {
        _clock.Advance(tickDuration);
        bus.Publish(Envelope<ClockTicked>.Root(new ClockTicked(_clock.ElapsedSeconds)));
    }
}
