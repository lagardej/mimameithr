using Friflo.Engine.ECS;
using Godot;
using Kjarni.Nornir;
using Skald.Bithot.Eldr.Irradiance;
using Skald.Bithot.Geimr.Geometry;
using Skald.Bithot.Geimr.Orbit;

namespace Skald.Bithot;

/// <summary>
///     Front engine for Bithot. Instantiates Bithot systems against a shared <see cref="EntityStore" />,
///     advances render-side synchronization when requested, and attaches scene-level renderers to a Godot node
///     tree.
/// </summary>
/// <remarks>
///     Domain simulation stays in <c>Kjarni.Nornir.Nornir</c>. Bithot only derives and updates render components
///     from domain state already present in the same store.
/// </remarks>
public sealed class Bithot
{
    private readonly Nornir _nornir;
    private readonly EntityStore _store;
    private readonly IReadOnlyList<IBithotSystem> _systems;

    /// <summary>Creates a Bithot engine on top of an existing shared <paramref name="store" />.</summary>
    public Bithot(EntityStore store, Nornir nornir)
    {
        _store = store;
        _nornir = nornir;
        _systems = BithotSystemRegistry.Build(store);
    }

    /// <summary>Advances all Bithot systems that need frame-level synchronization.</summary>
    public void Advance()
    {
        foreach (var system in _systems) system.Advance();
    }

    /// <summary>Attaches body, lighting, and camera renderers to <paramref name="parent" />.</summary>
    public void AttachTo(Node parent, Vector3? lightDirection = null)
    {
        new BodyRenderer(_nornir, _store).AttachTo(parent);
        new IrradianceRenderer().AttachStellarLight(parent, lightDirection ?? new Vector3(-1f, -0.6f, -0.4f));
        new OrbitRenderer(_nornir).AttachCamera(parent);
    }
}