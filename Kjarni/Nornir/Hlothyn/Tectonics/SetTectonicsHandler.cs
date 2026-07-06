using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Hlothyn.Tectonics;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Ginnungagap.Seed;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Tectonics;

/// <summary>Handles <see cref="SetTectonics" /> commands against the entity store.</summary>
public class SetTectonicsHandler(EntityStore store) : ICommandHandler<SetTectonics>
{
    /// <summary>The command type</summary>
    public static Type CommandType => typeof(SetTectonics);

    /// <inheritdoc />
    public void Handle(SetTectonics command)
    {
        var entity = store.GetEntityById(command.Id);
        var geometry = entity.GetComponent<GeometryC>();
        var physics = entity.GetComponent<PhysicsC>();
        var seed = store.GetUniqueEntity(SeedC.Uid).GetComponent<SeedC>().Seed;
        var hash = Hashing.StableHash(seed, (ulong) command.Id, 0, 0);

        var parameters = command.ToParameters(physics, geometry, hash);
        var result = TectonicsSimulation.Run(parameters);

        store.Query<CellIdentityC, CellParentRefC>().ForEachEntity((ref identity, ref parentRef, cellEntity) =>
        {
            if (parentRef.Parent != entity || !result.Cells.TryGetValue(identity.Id, out var cell))
            {
                return;
            }

            cellEntity.AddComponent(cell.ToComponent());
        });
    }
}

internal static class Extensions
{
    extension(SetTectonics command)
    {
        public TectonicsParameters ToParameters(PhysicsC physics, GeometryC geometry, uint seed) => new()
        {
            BodyAge = physics.Age,
            BodyMass = physics.Mass,
            BodyRadius = geometry.Radius,
            BodySurfaceGravity = physics.Gravity,
            BoundaryFocus = Ratio.FromDecimalFractions(Scaling.Range10.LinearScale(command.BoundaryFocus, 0, 1)),
            CollisionDominance =
                Ratio.FromDecimalFractions(Scaling.Range10.LinearScale(command.CollisionDominance, 0, 1)),
            Grid = GridProvider.Get(geometry.GridShape),
            HotSpotDensity = Ratio.FromDecimalFractions(Scaling.Range10.LinearScale(command.HotSpotDensity, 0, 1)),
            PlateCount = (int) Math.Round(Scaling.Range10.LinearScale(command.PlateCount, 4, 50)),
            PlateFragmentation =
                Ratio.FromDecimalFractions(Scaling.Range10.LinearScale(command.PlateFragmentation, 0, 1)),
            PlateStability = Ratio.FromDecimalFractions(Scaling.Range10.LinearScale(command.PlateStability, 0, 1)),
            Seed = seed
        };
    }

    extension(TectonicsCell cell)
    {
        public TectonicsC ToComponent() => new()
        {
            BoundaryType = cell.BoundaryType,
            CrustComposition = cell.CrustComposition,
            CrustalThickness = cell.CrustThickness,
            SeedCellId = cell.SeedCellId,
            VerticalDisplacementRate = cell.VerticalDisplacementRate
        };
    }
}
