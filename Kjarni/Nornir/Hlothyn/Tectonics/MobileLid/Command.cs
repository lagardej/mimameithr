using Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>Command to configure the tectonics of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="BoundaryFocus">Degree to which activity concentrates along plate boundaries. Range: 1–10.</param>
/// <param name="CollisionDominance">Bias toward convergent boundaries. Range: 1–10.</param>
/// <param name="HotSpotDensity">Number and intensity of intraplate volcanic plumes. Range: 1–10.</param>
/// <param name="PlateCount">Number of tectonic plates seeded at world gen. Range: 1–10.</param>
/// <param name="PlateFragmentation">Plate size distribution. Range: 1–10.</param>
/// <param name="PlateStability">Rate at which plates reorganize over time. Range: 1–10.</param>
public record SetTectonicsMobileLid(
    int Id,
    [Range(1u, 10u)] uint BoundaryFocus,
    [Range(1u, 10u)] uint CollisionDominance,
    [Range(1u, 10u)] uint HotSpotDensity,
    [Range(1u, 10u)] uint PlateCount,
    [Range(1u, 10u)] uint PlateFragmentation,
    [Range(1u, 10u)] uint PlateStability
) : ICommand;
