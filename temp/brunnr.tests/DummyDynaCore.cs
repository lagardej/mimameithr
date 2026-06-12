using Brunnr.Dynacore;

namespace Brunnr.Tests;

public sealed record DummyParameters : IParameters;

public sealed record DummyState : IState;

public sealed class DummyDynaCore : IDynaCore<DummyParameters, DummyState>
{
    public int FieldCount => 0;

    public DummyState PrepareState(CellIndex cellIndex, DummyParameters parameters, DummyState previousState)
        => previousState;

    public void ComputeCell(
        ComputeContext context,
        CellComputeContext cellContext,
        DummyParameters parameters,
        DummyState workingState,
        double[][] fields)
    {
        // No fields to update.
    }

    public DynaCoreResult<DummyState> AssembleResult(
        ComputeContext context,
        DummyParameters parameters,
        DummyState workingState,
        double[][] fields,
        TimeSpan computeElapsed)
        => new(workingState, []);
}
