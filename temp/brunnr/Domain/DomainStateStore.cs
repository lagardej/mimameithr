using Brunnr.Component;
using Brunnr.Dynacore;

namespace Brunnr.Domain;

/// <summary>
///     Typed store for one state type within all domains and cell sets.
///     Components use this to read previous state before ticking and persist the new state after ticking.
///     Implementations must be thread-safe for parallel component execution.
/// </summary>
public interface IDomainStateStore<TState>
    where TState : class, IState
{
    /// <summary>Retrieve state for a domain and cell set. Returns null when no value exists yet.</summary>
    TState? Get(DomainId domain, CellSet cells);

    /// <summary>Store or update state for a domain and cell set.</summary>
    void Set(DomainId domain, CellSet cells, TState state);
}
