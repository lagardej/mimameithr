using Brunnr.Dispatch;
using Brunnr.Domain;
using Brunnr.Dynacore;
using Brunnr.Messaging;
using Brunnr.Schedule;
using Brunnr.Topology;
using System.Diagnostics;

namespace Brunnr.Component;

/// <summary>
///     Non-generic base for all components. Exposes <see cref="ScheduleFor{TCause}" /> so the engine and registry
///     can operate on components without knowledge of their type parameters.
/// </summary>
public abstract class BaseComponent(IMessageBus bus)
{
    protected abstract ulong Cadence { get; }

    /// <summary>
    ///     Registers a job with the <see cref="Scheduler" />. <paramref name="cause" /> is required so every
    ///     scheduling — initial or rescheduled — carries a traceable causation back to the originating
    ///     <see cref="EngineStarted" /> or <see cref="JobStarted" />.
    /// </summary>
    public void ScheduleFor<TCause>(DomainId domain, ulong dueAt, Envelope<TCause> cause, CellSet? cellSet = null)
    {
        var job = new JobScheduled(GetType().Name, domain, dueAt, OnTick, cellSet ?? CellSet.Global);
        bus.Publish(cause.CausedBy(job));
    }

    protected abstract void OnTick(TickContext context);
}

/// <summary>
///     Typed base for components. Stateless: reads previous state from <see cref="IDomainStateStore{TState}" />,
///     delegates advancement to <see cref="IDynaCore{TParameters,TState}" /> cell by cell, then writes new state back.
///     Components are singleton and domain-agnostic. Domain context is provided per tick via <see cref="TickContext" />.
///     Initial scheduling is orchestrated by the bootstrapper (see <see cref="Engine" />).
/// </summary>
public abstract class BaseComponent<TParameters, TState>(
    IMessageBus bus,
    IDomainStateStore<TState> stateStore,
    IDynaCore<TParameters, TState> dynaCore,
    ICellIndexRegistry cellIndexRegistry,
    IGrid grid)
    : BaseComponent(bus)
    where TParameters : IParameters
    where TState : class, IState
{
    private readonly IMessageBus _bus = bus;

    /// <summary>State used on the first tick when no persisted state exists for the domain yet.</summary>
    protected abstract TState InitialState { get; }

    /// <summary>
    ///     Resolves simulation parameters for the current domain.
    ///     Called on every tick so operator updates can be applied without component mutation.
    /// </summary>
    protected abstract TParameters ResolveParameters(DomainId domain);

    protected override void OnTick(TickContext context)
    {
        var parameters = ResolveParameters(context.Domain);
        var previousState = stateStore.Get(context.Domain, context.CellSet) ?? InitialState;
        var cellIndex = cellIndexRegistry.Get(context.Domain, context.CellSet);
        var computeContext = new ComputeContext(grid, cellIndex, Cadence);
        var workingState = dynaCore.PrepareState(cellIndex, parameters, previousState);

        var fields = new double[dynaCore.FieldCount][];
        for (var i = 0; i < fields.Length; i++)
        {
            fields[i] = cellIndex.RentField();
        }

        try
        {
            var sw = Stopwatch.StartNew();
            for (var ordinal = 0; ordinal < cellIndex.Count; ordinal++)
            {
                var cell = cellIndex.CellAt(ordinal);
                var neighborOrdinals = ResolveNeighborOrdinals(cell, cellIndex);
                var cellContext = new CellComputeContext(ordinal, neighborOrdinals);
                dynaCore.ComputeCell(computeContext, cellContext, parameters, workingState, fields);
            }

            sw.Stop();

            var result = dynaCore.AssembleResult(computeContext, parameters, workingState, fields, sw.Elapsed);
            stateStore.Set(context.Domain, context.CellSet, result.State);

            var componentTicked =
                new ComponentTicked(GetType().Name, context.ElapsedSeconds, sw.Elapsed, result.Metrics);
            _bus.Publish(context.Cause.CausedBy(componentTicked));
            ScheduleFor(context.Domain, context.ElapsedSeconds + Cadence, context.Cause, context.CellSet);
        }
        finally
        {
            foreach (var field in fields)
            {
                CellIndex.ReturnField(field);
            }
        }
    }

    /// <summary>
    ///     Returns neighbor ordinals for <paramref name="cell" /> that are present in <paramref name="cellIndex" />,
    ///     sorted ascending for deterministic ordering (Invariant D).
    /// </summary>
    private List<int> ResolveNeighborOrdinals(CellId cell, CellIndex cellIndex)
    {
        var ordinals = new List<int>();
        foreach (var neighbor in grid.GridRing(cell, 1))
        {
            if (cellIndex.TryOrdinal(neighbor, out var ord))
            {
                ordinals.Add(ord);
            }
        }

        ordinals.Sort();
        return ordinals;
    }
}
