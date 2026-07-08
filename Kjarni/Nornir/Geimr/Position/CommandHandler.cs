using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Position;

/// <summary>Handles <see cref="SetPosition" /> commands against the entity store.</summary>
public class SetPositionHandler(EntityStore store) : ICommandHandler<SetPosition>
{
    /// <inheritdoc />
    public void Handle(SetPosition command)
    {
        var entity = store.GetEntityById(command.Id);

        entity.AddComponent(new PositionC
        {
            X = Length.FromKilometers(command.X),
            Y = Length.FromKilometers(command.Y),
            Z = Length.FromKilometers(command.Z),
            VelocityX = Speed.FromKilometersPerSecond(command.VelocityX),
            VelocityY = Speed.FromKilometersPerSecond(command.VelocityY),
            VelocityZ = Speed.FromKilometersPerSecond(command.VelocityZ)
        });
    }
}
