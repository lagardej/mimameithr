using Kjarni.Brunnr.Command;
using System.ComponentModel.DataAnnotations;

namespace Kjarni.Nornir.Geimr.Rotation;

/// <summary>Command to configure the rotational properties of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="InitialAngle">Initial angle of rotation, in degrees. Range: [1, 360].</param>
/// <param name="RotationRate">Duration of one full rotation on a 1-100 scale mapping 10⁹ to 10 s exponentially.</param>
/// <remarks>
///     <see href="../../../../docs/Nornir/Geimr/Rotation-rotationRate-scale.adoc">Full scale reference</see>
/// </remarks>
public record SetRotation(
    int Id,
    [Range(1u, 360u)] uint InitialAngle,
    [Range(1u, 100u)] uint RotationRate
) : ICommand;
