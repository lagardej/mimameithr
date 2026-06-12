namespace Brunnr.Dynacore;

/// <summary>Output of a single <see cref="IDynaCore{TParameters,TState}" /> tick: new state and diagnostic metrics.</summary>
public record DynaCoreResult<TState>(TState State, IReadOnlyList<DynaCoreMetric> Metrics)
    where TState : IState;
