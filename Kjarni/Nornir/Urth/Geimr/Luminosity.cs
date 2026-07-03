using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the luminosity of a star.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Luminosity">
///     Total power radiated across all wavelengths, in thousandths of L☉.
///     Range: [1, 80 000], covering stellar classes M through A.
/// </param>
public record SetLuminosity(
    int Id,
    [Range(1u, 80_000u)] uint Luminosity
) : ICommand;

/// <summary>Handles <see cref="SetLuminosity" /> commands against the entity store.</summary>
public class SetLuminosityHandler(EntityStore store) : ICommandHandler<SetLuminosity>
{
    private const double SolarLuminosityWatts = 3.828e26;

    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetLuminosity);

    /// <inheritdoc />
    public void Handle(SetLuminosity command)
    {
        var entity = store.GetEntityById(command.Id);
        entity.AddComponent(new LuminosityC
        {
            Luminosity = Luminosity.FromWatts(command.Luminosity / 1000.0 * SolarLuminosityWatts)
        });
    }
}
