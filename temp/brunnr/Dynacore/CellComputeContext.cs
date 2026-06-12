namespace Brunnr.Dynacore;

/// <summary>
///     Per-cell invocation data passed to <see cref="IDynaCore{TParameters,TState}.ComputeCell" />.
///     This is a value type to avoid per-cell heap allocations.
/// </summary>
public readonly record struct CellComputeContext(int Ordinal, IReadOnlyList<int> NeighborOrdinals);
