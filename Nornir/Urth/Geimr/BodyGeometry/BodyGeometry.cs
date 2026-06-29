using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using System.ComponentModel.DataAnnotations;
using UnitsNet;

namespace Nornir.Urth.Geimr.BodyGeometry;

/// <summary>Physical geometry of a planetary body.</summary>
[ComponentKey("body-geometry-settings")]
[Component("Geimr/BodyGeometrySettings")]
public struct BodyGeometrySettingsC : IComponent
{
    /// <summary>Mean radius of the body.</summary>
    [Setting("m", "Mean radius of the body.")]
    public Length Radius;

    /// <summary>Angle between the body's rotational axis and its orbital plane normal.</summary>
    [Setting("°", "Axial tilt relative to the orbital plane normal.")]
    public Angle AxialTilt;
}

/// <summary>Command to configure the geometry of a planetary body.</summary>
/// <param name="Radius">Mean radius of the body, in km. Range: [500, 150 000].</param>
/// <param name="AxialTilt">Axial tilt relative to the orbital plane normal, in degrees. Range: [0, 180].</param>
public record BodyGeometry(
    [property: Range(500u, 150_000u)] uint Radius,
    [property: Range(0u, 180u)] uint AxialTilt
);

/// <summary>Handles <see cref="BodyGeometry" /> commands against the entity store.</summary>
public class BodyGeometryHandler(EntityStore store) : ICommandHandler
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(BodyGeometry);

    /// <inheritdoc />
    public void Handle(int id, object command) => Handle(id, (BodyGeometry) command);

    /// <summary>Replaces the <see cref="BodyGeometrySettingsC" /> on the given entity.</summary>
    private void Handle(int id, BodyGeometry command)
    {
        var entity = store.GetEntityById(id);
        entity.AddComponent(new BodyGeometrySettingsC
        {
            Radius = Length.FromKilometers(command.Radius), AxialTilt = Angle.FromDegrees(command.AxialTilt)
        });
    }
}
