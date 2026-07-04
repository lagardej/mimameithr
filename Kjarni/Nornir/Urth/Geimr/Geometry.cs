using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Grid;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using static Kjarni.Kvasir.Formal.Maths.Scaling;

namespace Kjarni.Nornir.Urth.Geimr;

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

/// <summary>Handles <see cref="SetGeometry" /> commands against the entity store.</summary>
public class SetGeometryHandler(EntityStore store) : ICommandHandler<SetGeometry>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetGeometry);

    /// <inheritdoc />
    public void Handle(SetGeometry command)
    {
        var entity = store.GetEntityById(command.Id);
        var radius = Range100.ExponentialScale(command.Radius, 1, 1e9);

        entity.AddComponent(new GeometryC
        {
            AxialTilt = Angle.FromDegrees(command.AxialTilt),
            GridShape = command.GridShape,
            Radius = Length.FromKilometers(radius)
        });
    }
}
