using Brunnr.Engine;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;

namespace Nornir;

/// <summary>
///     Headless simulation engine. Drives the <see cref="SystemRoot" /> tick loop.
/// </summary>
public class Nornir : BaseEngine
{
    protected override SystemRoot BuildRoot(EntityStore store) => NornirSystems.Build(store);
}
