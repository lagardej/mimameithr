using Friflo.Engine.ECS;

namespace Kjarni.Nornir;

/// <summary>Front engine. Owns the shared <see cref="EntityStore" /> and instantiates the phase engines.</summary>
public class Nornir
{
    private readonly EntityStore _store = new();

    /// <summary>Constructor</summary>
    public Nornir()
    {
        Urðr = new Urðr(_store);
        Verðandi = new Verðandi(_store);
    }

    /// <summary>The Norn of the past. Name of the generation phase — the world before sentient civilization begins.</summary>
    public Urðr Urðr { get; }

    /// <summary>The Norn of the present. Name of the active phase engine — the world as it unfolds.</summary>
    public Verðandi Verðandi { get; }

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
