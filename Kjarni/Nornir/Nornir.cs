using Friflo.Engine.ECS;
using Kjarni.Nornir.Ginnungagap.Seed;

namespace Kjarni.Nornir;

/// <summary>Front engine. Owns the shared <see cref="EntityStore" /> and instantiates the phase engines.</summary>
public class Nornir
{
    private readonly RandomProvider _randomProvider = new();
    private readonly EntityStore _store = new();

    /// <summary>The Norn of the past. Name of the generation phase — the world before sentient civilization begins.</summary>
    private readonly Urðr Urðr;

    /// <summary>The Norn of the present. Name of the active phase engine — the world as it unfolds.</summary>
    private readonly Verðandi Verðandi;

    /// <summary>Constructor</summary>
    public Nornir()
    {
        Urðr = new Urðr(_store, _randomProvider);
        Verðandi = new Verðandi(_store);
    }

    #region Queries

    /// <summary>Returns all components attached to the entity with the given <paramref name="id" />.</summary>
    public EntityComponents GetComponents(int id) => _store.GetEntityById(id).Components;

    /// <summary>
    ///     Returns the component of type <typeparamref name="T" /> attached to the entity with the given
    ///     <paramref name="id" />.
    /// </summary>
    public T GetComponent<T>(int id) where T : struct, IComponent => _store.GetEntityById(id).GetComponent<T>();

    #endregion
}
