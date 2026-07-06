using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Eldr.Luminosity;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Orbit;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Geimr.Rotation;
using Kjarni.Nornir.Ginnungagap.Seed;
using Kjarni.Nornir.Hlothyn.Tectonics;

namespace Kjarni.Nornir;

internal class HandlerRegistry(EntityStore store)
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

    internal void Dispatch<TCommand>(TCommand command) where TCommand : ICommand
    {
        if (!_handlers.TryGetValue(typeof(TCommand), out var handler))
        {
            throw new InvalidOperationException($"No handler registered for command type {typeof(TCommand).Name}.");
        }

        handler.Handle(command);
    }
}
