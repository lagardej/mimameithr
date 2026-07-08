using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Time;
using Kjarni.Nornir.Ginnungagap.Seed;

namespace Kjarni.Nornir;

/// <summary>Front engine. Owns the shared <see cref="EntityStore" /> and instantiates the phase engines.</summary>
public class Nornir
{
    private readonly RandomProvider _randomProvider = new();
    private readonly EntityStore _store = new();

    /// <summary>The Norn of the past. Name of the generation phase — the world before sentient civilization begins.</summary>
    private readonly Urðr _urðr;

    /// <summary>The Norn of the present. Name of the active phase engine — the world as it unfolds.</summary>
    private readonly Verðandi _verðandi;

    /// <summary>Constructor</summary>
    public Nornir()
    {
        _urðr = new Urðr(_store, _randomProvider);
        _verðandi = new Verðandi(_store);
    }

    /// <summary>Current time compression factor. Defaults to <see cref="TimeCompression.RealTime" />.</summary>
    public TimeCompression Compression { get; set; } = TimeCompression.RealTime;

    #region Commands

    /// <summary>Creates a new entity and returns its id.</summary>
    public int CreateEntity() => _store.CreateEntity().Id;

    /// <summary>
    ///     Validates and routes a command to the generation-phase endpoint registered for
    ///     <typeparamref name="TCommand" />.
    /// </summary>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Thrown when the command validation fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no endpoint is registered for <typeparamref name="TCommand" />.</exception>
    public void Handle<TCommand>(TCommand command) where TCommand : ICommand => _urðr.Handle(command);

    /// <summary>Advances the simulation by <paramref name="deltaTime" /> real seconds, scaled by <see cref="Compression" />.</summary>
    public void Advance(float deltaTime) => _verðandi.Advance(deltaTime, Compression);

    #endregion

    #region Queries

    /// <summary>Returns all components attached to the entity with the given <paramref name="id" />.</summary>
    public EntityComponents GetComponents(int id) => _store.GetEntityById(id).Components;

    /// <summary>
    ///     Returns the component of type <typeparamref name="T" /> attached to the entity with the given
    ///     <paramref name="id" />.
    /// </summary>
    public T GetComponent<T>(int id) where T : struct, IComponent => _store.GetEntityById(id).GetComponent<T>();

    /// <summary>Returns the ids of all entities having a component of type <typeparamref name="T" />.</summary>
    public IEnumerable<int> Query<T>() where T : struct, IComponent =>
        _store.Query<T>().Entities.Select(e => e.Id);

    #endregion
}
