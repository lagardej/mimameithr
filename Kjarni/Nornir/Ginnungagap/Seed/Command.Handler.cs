using Brunnr.Command;

namespace Nornir.Ginnungagap.Seed;

/// <summary>
///     Handles <see cref="SetSeed" /> commands against <see cref="RandomProvider" />.
/// </summary>
/// <param name="provider">Random provider.</param>
public class SetSeedHandler(RandomProvider provider) : ICommandHandler<SetSeed>
{
    /// <inheritdoc />
    public void Handle(SetSeed command) => provider.SetSeed(command.Seed);
}
