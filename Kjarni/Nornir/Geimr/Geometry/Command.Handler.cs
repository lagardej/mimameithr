using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using UnitsNet;
using static Kjarni.Nornir.Geimr.Geometry.SetGeometryScale;

namespace Kjarni.Nornir.Geimr.Geometry;

/// <summary>Handles <see cref="SetGeometry" /> commands against the entity store.</summary>
public class SetGeometryHandler(EntityStore store) : ICommandHandler<SetGeometry>
{
    /// <inheritdoc />
    public void Handle(SetGeometry command)
    {
        var entity = store.GetEntityById(command.Id);
        var radius = RadiusScale.Evaluate(command.Radius);

        entity.AddComponent(new GeometryC
        {
            AxialTilt = Angle.FromDegrees(command.AxialTilt),
            GridShape = command.GridShape,
            Radius = Length.FromKilometers(radius)
        });
    }
}
