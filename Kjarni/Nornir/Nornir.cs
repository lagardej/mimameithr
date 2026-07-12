using Brunnr.Command;
using Brunnr.Time;
using Friflo.Engine.ECS;
using Nornir.Ginnungagap.Seed;

namespace Nornir;

/// <summary>Front engine. Instantiates the phase engines against an injected or private <see cref="EntityStore" />.</summary>
public class Nornir : IDisposable
{
    /// <summary>
    ///     Only set (and owned) when using the private-store constructor below — an injected store's JobRunner is the
    ///     caller's responsibility (see Brunnr.Engine.BrunnrEngine).
    /// </summary>
    private readonly ParallelJobRunner? _ownedJobRunner;

    private readonly RandomProvider _randomProvider = new();
    private readonly EntityStore _store;

    /// <summary>The Norn of the past. Name of the generation phase — the world before sentient civilization begins.</summary>
    private readonly Urðr _urðr;

    /// <summary>The Norn of the present. Name of the active phase engine — the world as it unfolds.</summary>
    private readonly Verðandi _verðandi;

    /// <summary>
    ///     Constructor. Injected with a shared <see cref="EntityStore" /> — see <see cref="Brunnr.BrunnrEngine" />
    ///     .
    /// </summary>
    public Nornir(EntityStore store)
    {
        _store = store;
        _urðr = new Urðr(store, _randomProvider);
        _verðandi = new Verðandi(store);
    }

    /// <summary>Constructor. Owns a private, unshared store — for headless use with no other engine attached.</summary>
    public Nornir() : this(CreateHeadlessStore(out var runner)) => _ownedJobRunner = runner;

    /// <summary>Current time compression factor. Defaults to <see cref="TimeCompression.RealTime" />.</summary>
    public TimeCompression Compression { get; set; } = TimeCompression.RealTime;

    /// <inheritdoc />
    public void Dispose() => _ownedJobRunner?.Dispose();

    /// <summary>Parallel query jobs (e.g. IrradianceSystem) require a ParallelJobRunner assigned to the store.</summary>
    private static EntityStore CreateHeadlessStore(out ParallelJobRunner runner)
    {
        runner = new ParallelJobRunner(Environment.ProcessorCount);
        return new EntityStore { JobRunner = runner };
    }

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
