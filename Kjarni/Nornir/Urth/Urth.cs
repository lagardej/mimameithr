using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Data.Validation;
using Kjarni.Nornir.Urth.Geimr;
using Kjarni.Nornir.Urth.Ginnungagap;
using Kjarni.Nornir.Urth.Hlothyn;
using Kjarni.Nornir.Wyrd.Ginnungagap;

namespace Kjarni.Nornir.Urth;

/// <summary>Generation phase engine. Routes commands to registered endpoints.</summary>
public class Urðr(EntityStore store)
{
    private readonly Dictionary<Type, ICommandHandler> _handlers = RegisterHandlers(store);

    private static Dictionary<Type, ICommandHandler> RegisterHandlers(EntityStore store) => new()
    {
        { SetGeometryHandler.CommandType, new SetGeometryHandler(store) },
        { SetLuminosityHandler.CommandType, new SetLuminosityHandler(store) },
        { SetOrbitHandler.CommandType, new SetOrbitHandler(store) },
        { SetPhysicsHandler.CommandType, new SetPhysicsHandler(store) },
        { SetRotationHandler.CommandType, new SetRotationHandler(store) },
        { SetSeedHandler.CommandType, new SetSeedHandler(store) },
        { SetTectonicsHandler.CommandType, new SetTectonicsHandler(store) }
    };

    #region Commands

    /// <summary>Creates a new entity and returns its id.</summary>
    public int CreateEntity() => store.CreateEntity().Id;

    /// <summary>Creates the universe entity and sets its seed. Only one universe entity may exist per store.</summary>
    public int CreateUniverse(uint seed)
    {
        var entity = store.CreateEntity();
        entity.AddComponent(new UniqueEntity(SeedC.Uid));
        Handle(new SetSeed(entity.Id, seed));
        return entity.Id;
    }

    /// <summary>Validates and routes a command to the endpoint registered for <typeparamref name="TCommand" />.</summary>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Thrown when the command validation fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no endpoint is registered for <typeparamref name="TCommand" />.</exception>
    public void Handle<TCommand>(TCommand command) where TCommand : ICommand
    {
        Validator.Validate(command);

        if (!_handlers.TryGetValue(typeof(TCommand), out var handler))
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name}.");
        }

        handler.Handle(command);
    }

    #endregion

    #region Queries

    /// <summary>Returns all components attached to the entity with the given <paramref name="id" />.</summary>
    public EntityComponents GetComponents(int id) => store.GetEntityById(id).Components;

    /// <summary>
    ///     Returns the component of type <typeparamref name="T" /> attached to the entity with the given
    ///     <paramref name="id" />.
    /// </summary>
    public T GetComponent<T>(int id) where T : struct, IComponent => store.GetEntityById(id).GetComponent<T>();

    #endregion
}
