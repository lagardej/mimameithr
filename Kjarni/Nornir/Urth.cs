using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Data.Validation;
using Kjarni.Nornir.Ginnungagap.Seed;

namespace Kjarni.Nornir;

/// <summary>Generation phase engine. Routes commands to registered endpoints.</summary>
public class Urðr(EntityStore store)
{
    private readonly HandlerRegistry _handlerRegistry = new(store);

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
        _handlerRegistry.Dispatch(command);
    }
}
