using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Geimr.Geometry;

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
