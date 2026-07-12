using Brunnr.Command;
using Friflo.Engine.ECS;
using static Nornir.Eldr.Luminosity.SetLuminosityScale;

namespace Nornir.Eldr.Luminosity;

/// <summary>Handles <see cref="SetLuminosity" /> commands against the entity store.</summary>
public class SetLuminosityHandler(EntityStore store) : ICommandHandler<SetLuminosity>
{
    private const double SolarLuminosityWatts = 3.828e26;

    /// <inheritdoc />
    public void Handle(SetLuminosity command)
    {
        var entity = store.GetEntityById(command.Id);
        var luminosity = LuminosityScale.Evaluate(command.Luminosity) * SolarLuminosityWatts;

        entity.AddComponent(new LuminosityC { Luminosity = UnitsNet.Luminosity.FromWatts(luminosity) });
    }
}
