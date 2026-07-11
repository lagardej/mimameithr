using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Kvasir.Foundation;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Eldr.Luminosity;

/// <summary>Handles <see cref="SetLuminosity" /> commands against the entity store.</summary>
public class SetLuminosityHandler(EntityStore store) : ICommandHandler<SetLuminosity>
{
    private const double SolarLuminosityWatts = 3.828e26;

    private static readonly PiecewiseExponentialScale s_luminosityScale =
        new(Range1000, [-3, 0, 1, 3], [400, 700, 1000]);

    /// <inheritdoc />
    public void Handle(SetLuminosity command)
    {
        var entity = store.GetEntityById(command.Id);
        var luminosity = s_luminosityScale.Evaluate(command.Luminosity) * SolarLuminosityWatts;

        entity.AddComponent(new LuminosityC { Luminosity = UnitsNet.Luminosity.FromWatts(luminosity) });
    }
}
