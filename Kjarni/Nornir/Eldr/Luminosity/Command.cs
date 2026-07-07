using Kjarni.Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Eldr.Luminosity;

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
