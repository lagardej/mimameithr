using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Eldr.Luminosity;

/// <summary>Handles <see cref="SetLuminosity" /> commands against the entity store.</summary>
public class SetLuminosityHandler(EntityStore store) : ICommandHandler<SetLuminosity>
{
    private const double SolarLuminosityWatts = 3.828e26;

    /// <inheritdoc />
    public void Handle(SetLuminosity command)
    {
        var entity = store.GetEntityById(command.Id);
        var luminosity = Range100.ExponentialScale(command.Luminosity, 1e-3, 1e3) * SolarLuminosityWatts;

        entity.AddComponent(new LuminosityC { Luminosity = UnitsNet.Luminosity.FromWatts(luminosity) });
    }
}
