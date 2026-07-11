using Friflo.Engine.ECS;

namespace Skald.Bithot;

/// <summary>
///     Front engine for Bithot. Instantiates Bithot systems against a shared <see cref="EntityStore" /> and
///     advances render-side synchronization when requested.
/// </summary>
/// <remarks>
///     Domain simulation stays in <c>Kjarni.Nornir.Nornir</c>. Bithot only derives and updates render components
///     from domain state already present in the same store.
/// </remarks>
public sealed class Bithot
{
    private readonly IReadOnlyList<IBithotSystem> _systems;

    /// <summary>Creates a Bithot engine on top of an existing shared <paramref name="store" />.</summary>
    public Bithot(EntityStore store)
    {
        _systems = BithotSystemRegistry.Build(store);
    }

    /// <summary>Advances all Bithot systems that need frame-level synchronization.</summary>
    public void Advance()
    {
        foreach (var system in _systems)
        {
            system.Advance();
        }
    }
}
