using Kjarni.Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Geimr.Orbit;

/// <summary>Command to configure the orbital parameters of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Eccentricity">Shape of the orbital ellipse, from circular to near-parabolic. Range: [1, 100].</param>
/// <param name="InitialMeanAnomaly">Position on the orbit at epoch, in degrees. Range: [1, 360].</param>
/// <param name="OrbitalPeriod">Sidereal orbital period on a 1–100 scale mapping 10³ s to 10¹³ s exponentially.</param>
/// <param name="SemiMajorAxis">Semi-major axis of the orbit on a 1–100 scale mapping 10⁴ km to 10¹⁴ km exponentially.</param>
/// <param name="ParentId">The parent id. May be null</param>
[Obsolete("Use Position instead")]
public record SetOrbit(
    int Id,
    [Range(1u, 100u)] uint Eccentricity,
    [Range(1u, 360u)] uint InitialMeanAnomaly,
    [Range(1u, 100u)] uint OrbitalPeriod,
    [Range(1u, 100u)] uint SemiMajorAxis,
    int? ParentId = null
) : ICommand;
