using Friflo.Engine.ECS;

namespace Kjarni.Brunnr.Engine;

/// <summary>
///     Owns the shared <see cref="EntityStore" /> backing every sub-engine attached to it. Sub-engines
///     (domain, rendering, ...) are constructed with this store injected, so they operate on the same
///     entities without needing to know about each other. Brunnr never references those sub-engine types
///     — composition happens one layer up, at whichever engine is the application's entry point.
/// </summary>
public sealed class BrunnrEngine : IDisposable
{
    // Thread-safe, reusable across all query jobs (Friflo docs) — one shared instance for the store
    // rather than one per QueryJob.
    private readonly ParallelJobRunner _jobRunner = new(Environment.ProcessorCount);

    /// <summary>Creates the shared store and its parallel-query job runner.</summary>
    public BrunnrEngine() => Store = new EntityStore { JobRunner = _jobRunner };

    /// <summary>The shared entity store. Inject into sub-engine constructors.</summary>
    public EntityStore Store { get; }

    /// <inheritdoc />
    public void Dispose() => _jobRunner.Dispose();
}
