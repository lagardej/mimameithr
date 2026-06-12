namespace Nornir.Engine.System;

public interface IDynacore<TParams>
{
    DynacoreResult Compute(DynacoreRequest<TParams> request);
}

public abstract record DynacoreRequest<TParams>(TParams Params, CellSet Cells, double ElapsedSeconds);

public sealed record DynacoreResult(DynacoreStatus Status, DynacoreMetrics Metrics);

public enum DynacoreStatus
{
    /// <summary>Computation completed successfully.</summary>
    Ok,

    /// <summary>Required state was not yet available; job should be rescheduled.</summary>
    Deferred
}

public sealed class DynacoreMetrics;
