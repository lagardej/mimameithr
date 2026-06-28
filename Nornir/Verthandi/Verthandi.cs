using Brunnr.Engine;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Geodesy;

namespace Nornir.Verthandi;

/// <summary>
///     Headless simulation engine. Drives the <see cref="SystemRoot" /> tick loop.
/// </summary>
public class Verðandi : BaseEngine
{
    /// <inheritdoc />
    protected override SystemRoot BuildRoot(EntityStore store, IGeodesicGrid grid, int seed) =>
        VerðandiSystems.Build(store, grid, seed);
}
