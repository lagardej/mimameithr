using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Foundation;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Geimr.Geometry;

/// <summary>Command to configure the geometry of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="GridShape">Grid backend shape for this body's discrete global grid.</param>
/// <param name="Radius">Mean radius on a 1–1000 scale mapping 1 km to 10⁹ km exponentially.</param>
/// <param name="AxialTilt">Axial tilt relative to the orbital plane normal, in degrees. Range: [0, 180].</param>
/// <remarks>
///     <para>Radius Scale Reference</para>
///     <code>
/// Scale |   Radius (km) | Closest Real Class
/// ----- | ------------- | ------------------
///     1 |             1 | Boulder
///   100 |            10 | Small asteroid
///   200 |            99 | Large asteroid
///   300 |           994 | Small moon
///   400 |        10,000 | Super-Earth
///   500 |        46,416 | Neptune-class planet
///   600 |       215,443 | Brown dwarf
///   700 |     1,000,000 | Sun-sized star (1.4 R☉)
///   800 |    10,000,000 | Red giant (14.3 R☉)
///   900 |   100,000,000 | Red supergiant (143 R☉)
///  1000 | 1,000,000,000 | Largest hypergiants (1,428 R☉)
/// </code>
/// </remarks>
public record SetGeometry(
    int Id,
    GridShape GridShape,
    [Range(1u, 1000u)] uint Radius,
    [Range(0u, 180u)] uint AxialTilt = 0
) : ICommand;

/// <summary>The scales used by the command properties.</summary>
public static class SetGeometryScale
{
    /// <summary>Radius scale: 1 km to 10⁹ km exponentially.</summary>
    public static readonly PiecewiseExponentialScale RadiusScale =
        new(Scaling.Range1000, [0, 4, 6, 9], [400, 700, 1000]);
}
