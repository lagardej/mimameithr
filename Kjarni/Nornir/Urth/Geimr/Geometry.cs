using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Nornir.Wyrd.Geimr;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Kjarni.Nornir.Urth.Geimr;

/// <summary>
///     Command to configure the geometry of a planetary body.
/// </summary>
/// <param name="Id">The entity id.</param>
/// <param name="AxialTilt">Axial tilt relative to the orbital plane normal, in degrees. Range: [0, 180].</param>
/// <param name="Radius">Mean radius on a 1–100 scale mapping 1 km to 10⁹ km exponentially.</param>
/// <remarks>
///     <list type="table">
///         <listheader>
///             <term>Scale</term>
///             <description>Radius (km)</description>
///         </listheader>
///         <item>
///             <term>1</term>
///             <description>1</description>
///         </item>
///         <item>
///             <term>12</term>
///             <description>10</description>
///         </item>
///         <item>
///             <term>23</term>
///             <description>100</description>
///         </item>
///         <item>
///             <term>34</term>
///             <description>1,000</description>
///         </item>
///         <item>
///             <term>45</term>
///             <description>10,000</description>
///         </item>
///         <item>
///             <term>56</term>
///             <description>100,000</description>
///         </item>
///         <item>
///             <term>67</term>
///             <description>1,000,000</description>
///         </item>
///         <item>
///             <term>78</term>
///             <description>10,000,000</description>
///         </item>
///         <item>
///             <term>89</term>
///             <description>100,000,000</description>
///         </item>
///         <item>
///             <term>100</term>
///             <description>1,000,000,000</description>
///         </item>
///     </list>
///     <see href="../../../../docs/Nornir/Geimr/BodyGeometry-radius-scale.adoc">Full scale reference</see>
/// </remarks>
public record SetGeometry(
    int Id,
    [Range(0u, 180u)] uint AxialTilt,
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
        var radiusKm = ScaleToKilometers(command.Radius);
        entity.AddComponent(new GeometryC
        {
            AxialTilt = Angle.FromDegrees(command.AxialTilt), Radius = Length.FromKilometers(radiusKm)
        });
    }

    /// <summary>Converts a 1–100 scale value to kilometres using exponential mapping: 1 km to 10⁹ km.</summary>
    /// <param name="scale">Scale value in range [1, 100].</param>
    /// <returns>Radius in kilometres.</returns>
    private static double ScaleToKilometers(uint scale) => Math.Pow(1e9, (scale - 1) / 99.0);
}
