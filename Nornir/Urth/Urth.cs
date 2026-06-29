using Brunnr.Engine.Data.Validation;
using Friflo.Engine.ECS;
using Nornir.Urth.Geimr.BodyGeometry;
using Nornir.Urth.Geimr.BodyOrbit;
using Nornir.Urth.Geimr.BodyPhysics;
using Nornir.Urth.Geimr.BodyRotation;
using Nornir.Urth.Geimr.StellarLuminosity;

namespace Nornir.Urth;

/// <summary>Generation phase engine. Routes commands to registered endpoints.</summary>
public class Urth(EntityStore store)
{
    private readonly Dictionary<Type, ICommandHandler> _handlers = Register(store);

    /// <summary>Creates a new entity and returns its id.</summary>
    public int CreateEntity() => store.CreateEntity().Id;

    /// <summary>
    ///     Validates and routes a command to the endpoint registered for <typeparamref name="T" />.
    /// </summary>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     Thrown when the command fails validation.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when no endpoint is registered for <typeparamref name="T" />.</exception>
    public void Configure<T>(int id, T command) where T : notnull
    {
        Validator.Validate(command);

        if (!_handlers.TryGetValue(typeof(T), out var handler))
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(T).Name}.");
        }

        handler.Handle(id, command);
    }

    /// <summary>Returns all components attached to the entity with the given <paramref name="id" />.</summary>
    public EntityComponents GetAll(int id) => store.GetEntityById(id).Components;

    /// <summary>
    ///     Returns the component of type <typeparamref name="T" /> attached to the entity with the given
    ///     <paramref name="id" />.
    /// </summary>
    public T GetComponent<T>(int id) where T : struct, IComponent => store.GetEntityById(id).GetComponent<T>();

    private static Dictionary<Type, ICommandHandler> Register(EntityStore store) => new()
    {
        { BodyGeometryHandler.CommandType, new BodyGeometryHandler(store) },
        { BodyPhysicsHandler.CommandType, new BodyPhysicsHandler(store) },
        { BodyRotationHandler.CommandType, new BodyRotationHandler(store) },
        { BodyOrbitHandler.CommandType, new BodyOrbitHandler(store) },
        { StellarLuminosityHandler.CommandType, new StellarLuminosityHandler(store) }
    };
}
