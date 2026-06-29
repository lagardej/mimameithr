using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Nornir.Urth.Geimr.StellarLuminosity;

/// <summary>Radiative output of a star.</summary>
[ComponentKey("stellar-luminosity-settings")]
[Component("Geimr/StellarLuminositySettings")]
public struct StellarLuminositySettingsC : IComponent
{
    /// <summary>Total power radiated across all wavelengths.</summary>
    [Setting("W", "Total power radiated across all wavelengths.")]
    public Luminosity Luminosity;
}

/// <summary>Command to configure the luminosity of a star.</summary>
/// <param name="Luminosity">
///     Total power radiated across all wavelengths, in thousandths of L☉.
///     Range: [1, 80 000], covering stellar classes M through A.
/// </param>
public record ConfigureStellarLuminosity(
    [property: Range(1u, 80_000u)] uint Luminosity
);

/// <summary>Handles <see cref="ConfigureStellarLuminosity" /> commands against the entity store.</summary>
public class StellarLuminosityHandler(EntityStore store) : ICommandHandler
{
    private const double SolarLuminosityWatts = 3.828e26;

    /// <summary>The command type</summary>
    public static Type CommandType => typeof(ConfigureStellarLuminosity);

    /// <inheritdoc />
    public void Handle(int id, object command) => Handle(id, (ConfigureStellarLuminosity) command);

    /// <summary>Replaces the <see cref="StellarLuminositySettingsC" /> on the given entity.</summary>
    private void Handle(int id, ConfigureStellarLuminosity command)
    {
        var entity = store.GetEntityById(id);
        entity.AddComponent(new StellarLuminositySettingsC
        {
            Luminosity = Luminosity.FromWatts(command.Luminosity / 1000.0 * SolarLuminosityWatts)
        });
    }
}
