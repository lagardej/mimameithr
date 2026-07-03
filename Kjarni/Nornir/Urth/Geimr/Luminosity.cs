using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the luminosity of a star.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Luminosity">Luminosity on a 1–100 scale mapping 10⁻³ L☉ to 10³ L☉ exponentially.</param>
/// <remarks>
///     <list type="table">
///         <listheader>
///             <term>Scale</term>
///             <description>Luminosity (L☉)</description>
///         </listheader>
///         <item>
///             <term>1</term>
///             <description>0.001</description>
///         </item>
///         <item>
///             <term>100</term>
///             <description>1,000</description>
///         </item>
///     </list>
///     <see href="../../../../docs/Nornir/Geimr/Luminosity-scale.adoc">Full scale reference</see>
/// </remarks>
public record SetLuminosity(
    int Id,
    [Range(1u, 100u)] uint Luminosity
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
        var luminositySolar = ScaleToSolar(command.Luminosity);
        entity.AddComponent(new LuminosityC
        {
            Luminosity = Luminosity.FromWatts(luminositySolar * SolarLuminosityWatts)
        });
    }

    /// <summary>Converts a 1–100 scale value to L☉ using exponential mapping: 10⁻³ to 10³.</summary>
    /// <param name="scale">Scale value in range [1, 100].</param>
    /// <returns>Luminosity in L☉.</returns>
    private static double ScaleToSolar(uint scale) => Math.Pow(10, -3 + 6.0 * (scale - 1) / 99.0);
}
