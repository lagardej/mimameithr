using Brunnr.Topology;

namespace Brunnr.Dynacore;

/// <summary>
///     Compute-time resources supplied to a DynaCore kernel each tick.
///     Passed to <see cref="IDynaCore{TParameters,TState}.ComputeCell" /> and
///     <see cref="IDynaCore{TParameters,TState}.AssembleResult" />.
/// </summary>
public sealed class ComputeContext(IGrid grid, CellIndex cellIndex, ulong deltaTime)
{
    /// <summary>Grid used for spatial queries.</summary>
    public IGrid Grid { get; } = grid;

    /// <summary>Stable ordinal index and pooled scratch fields for the active cell set.</summary>
    public CellIndex CellIndex { get; } = cellIndex;

    /// <summary>Simulated time elapsed since the previous tick, in seconds.</summary>
    public ulong DeltaTime { get; } = deltaTime;
}
