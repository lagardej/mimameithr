using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;

namespace Kjarni.Nornir.Ginnungagap.Seed;

/// <summary>
///     Handles <see cref="SetSeed" /> commands against the entity store.
/// </summary>
/// <param name="store"></param>
public class SetSeedHandler(EntityStore store) : ICommandHandler<SetSeed>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetSeed);

    /// <inheritdoc />
    public void Handle(SetSeed command)
    {
        var entity = store.GetEntityById(command.Id);
        entity.AddComponent(new SeedC { Seed = command.Seed });
    }
}
