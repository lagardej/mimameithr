using Brunnr.Engine;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Geodesy;

namespace Nornir;

/// <summary>
///     Headless simulation engine. Drives the <see cref="SystemRoot" /> tick loop.
/// </summary>
public class Nornir : BaseEngine
{
    /// <inheritdoc />
    protected override SystemRoot BuildRoot(EntityStore store, IGeodesicGrid grid, int seed) =>
        NornirSystems.Build(store, grid, seed);
}
