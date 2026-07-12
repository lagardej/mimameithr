using Brunnr.Command;
using Brunnr.Data.Validation;
using Friflo.Engine.ECS;
using Microsoft.Extensions.DependencyInjection;
using Nornir.Ginnungagap.Seed;
using System.Reflection;

namespace Nornir;

/// <summary>Generation phase engine. Routes commands to registered endpoints.</summary>
internal class Urðr(EntityStore store, RandomProvider randomProvider)
{
    private readonly HandlerRegistry _handlerRegistry = new(store, randomProvider);

    /// <summary>Validates and routes a command to the endpoint registered for <typeparamref name="TCommand" />.</summary>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Thrown when the command validation fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no endpoint is registered for <typeparamref name="TCommand" />.</exception>
    public void Handle<TCommand>(TCommand command) where TCommand : ICommand
    {
        Validator.Validate(command);
        _handlerRegistry.Dispatch(command);
    }
}

internal class HandlerRegistry
{
    private readonly Dictionary<Type, ICommandHandler> _handlers;

    internal HandlerRegistry(EntityStore store, RandomProvider randomProvider)
    {
        var services = new ServiceCollection();
        services.AddSingleton(store);
        services.AddSingleton(randomProvider);

        var handlerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Select(t => new { Type = t, CommandType = GetCommandType(t) })
            .Where(x => x.CommandType is not null)
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            services.AddSingleton(handlerType.Type);
        }

        var provider = services.BuildServiceProvider();

        _handlers = handlerTypes.ToDictionary(
            x => x.CommandType!,
            x => (ICommandHandler) provider.GetRequiredService(x.Type));
    }

    private static Type? GetCommandType(Type handlerType) => handlerType
        .GetInterfaces()
        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
        ?.GetGenericArguments()[0];

    internal void Dispatch<TCommand>(TCommand command) where TCommand : ICommand
    {
        if (!_handlers.TryGetValue(typeof(TCommand), out var handler))
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name}.");
        }

        handler.Handle(command);
    }
}
