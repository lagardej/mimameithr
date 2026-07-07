using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Grid;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Geimr.Geometry;

/// <summary>Command to configure the geometry of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="AxialTilt">Axial tilt relative to the orbital plane normal, in degrees. Range: [0, 180].</param>
/// <param name="GridShape">Grid backend shape for this body's discrete global grid.</param>
/// <param name="Radius">Mean radius on a 1–100 scale mapping 1 km to 10⁹ km exponentially.</param>
/// <remarks>
///     <see href="../../../../docs/Nornir/Geimr/BodyGeometry-radius-scale.adoc">Full scale reference</see>
/// </remarks>
public record SetGeometry(
    int Id,
    [Range(0u, 180u)] uint AxialTilt,
    GridShape GridShape,
    [Range(1u, 100u)] uint Radius
) : ICommand;
