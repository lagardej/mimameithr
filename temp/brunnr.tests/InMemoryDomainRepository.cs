using Brunnr.Component;
using Brunnr.Domain;
using Brunnr.Dynacore;

namespace Brunnr.Tests;

public sealed class InMemoryDomainStateStore<TState> : IDomainStateStore<TState>
    where TState : class, IState
{
    private readonly Dictionary<(DomainId Domain, CellSet Cells), TState> _store = new();

    public TState? Get(DomainId domain, CellSet cells)
        => _store.GetValueOrDefault((domain, cells));

    public void Set(DomainId domain, CellSet cells, TState state)
        => _store[(domain, cells)] = state;
}
