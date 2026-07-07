using Kjarni.Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Geimr.Physics;

/// <summary>Command to configure the physical properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Age">Age of the body, on a 1-100 scale mapping 10⁶ to 10¹³ years exponentially.</param>
/// <param name="Mass">Mass of the body on a 1-100 scale mapping 10¹² kg to 10³² kg exponentially.</param>
public record SetPhysics(
    int Id,
    [Range(1u, 100u)] uint Age,
    [Range(1u, 100u)] uint Mass
) : ICommand;
