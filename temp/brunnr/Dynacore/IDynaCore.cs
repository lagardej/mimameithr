namespace Brunnr.Dynacore;

/// <summary>Non-generic marker for <see cref="IDynaCore{TParameters,TState}" />. Used by the registry.</summary>
public interface IDynaCore
{
}

/// <summary>
///     Pure cell kernel. Stateless: maps (parameters, working state, cell context) → field writes.
///     Owns no mutable data; all compute resources flow in from the orchestration layer.
/// </summary>
public interface IDynaCore<in TParameters, TState> : IDynaCore
    where TParameters : IParameters
    where TState : IState
{
    /// <summary>Number of scratch <c>double[]</c> field buffers required per tick. Each has length <c>CellIndex.Count</c>.</summary>
    int FieldCount { get; }

    /// <summary>
    ///     Called once before cell iteration. May initialize first-tick state (e.g. plate seeding).
    ///     Returns the state that will be passed as <c>workingState</c> to <see cref="ComputeCell" /> and
    ///     <see cref="AssembleResult" />.
    /// </summary>
    TState PrepareState(CellIndex cellIndex, TParameters parameters, TState previousState);

    /// <summary>
    ///     Advances a single cell. Reads from <paramref name="workingState" /> (immutable previous values).
    ///     Writes to <c>fields[*][<paramref name="cellContext" />.Ordinal]</c> only — single-writer guarantee per Invariant A.
    ///     <paramref name="cellContext" />.NeighborOrdinals are provided in ascending order for determinism (Invariant D).
    ///     For symmetric pair contributions apply weight 0.5 (Invariant E).
    /// </summary>
    void ComputeCell(
        ComputeContext context,
        CellComputeContext cellContext,
        TParameters parameters,
        TState workingState,
        double[][] fields);

    /// <summary>
    ///     Assembles the new state and emits metrics from fully-computed <paramref name="fields" />.
    ///     Called once after all cells have been processed.
    /// </summary>
    DynaCoreResult<TState> AssembleResult(
        ComputeContext context,
        TParameters parameters,
        TState workingState,
        double[][] fields,
        TimeSpan computeElapsed);
}
