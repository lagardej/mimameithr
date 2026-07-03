using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Ginnungagap;

namespace Kjarni.Nornir.Urth.Ginnungagap;

/// <summary>World generation seed.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Seed">Seed</param>
public record SetSeed(
    int Id,
    uint Seed
) : ICommand;

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
