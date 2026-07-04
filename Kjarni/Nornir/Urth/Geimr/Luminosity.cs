using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using static Kjarni.Kvasir.Formal.Maths.Scaling;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>Command to configure the luminosity of a star.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Luminosity">Luminosity on a 1–100 scale mapping 10⁻³ L☉ to 10³ L☉ exponentially.</param>
/// <remarks>
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
        var luminosity = Range100.ExponentialScale(command.Luminosity, 1e-3, 1e3) * SolarLuminosityWatts;

        entity.AddComponent(new LuminosityC { Luminosity = Luminosity.FromWatts(luminosity) });
    }
}
