using Brunnr.Component;
using Brunnr.Domain;
using Brunnr.Dynacore;
using Brunnr.Messaging;
using Brunnr.Topology;

namespace Brunnr.Tests;

public sealed class DummyComponent(IMessageBus bus, IDomainStateStore<DummyState> stateStore, IGrid grid)
    : BaseComponent<DummyParameters, DummyState>(bus, stateStore, new DummyDynaCore(), new CellIndexRegistry(), grid)
{
    protected override ulong Cadence => 10;
    protected override DummyState InitialState => new();
    protected override DummyParameters ResolveParameters(DomainId domain) => new();
}
