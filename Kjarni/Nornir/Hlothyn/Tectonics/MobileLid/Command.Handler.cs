using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Brunnr.Grid;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Ginnungagap.Seed;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>Handles <see cref="SetTectonicsMobileLid" /> commands against the entity store.</summary>
public class SetTectonicsMobileLidHandler(EntityStore store, RandomProvider randomProvider)
    : ICommandHandler<SetTectonicsMobileLid>
{
    /// <inheritdoc />
    public void Handle(SetTectonicsMobileLid command)
    {
        var entity = store.GetEntityById(command.Id);
        RegimeInvariant.RemoveAllRegimeComponents(store, entity);
        entity.AddComponent(new TectonicsRegimeC { Regime = Regime.MobileLid });
        var geometry = entity.GetComponent<GeometryC>();
        var physics = entity.GetComponent<PhysicsC>();
        var rng = randomProvider.CreateStream((ulong) command.Id);

        var parameters = command.ToParameters(physics, geometry);
        var result = Simulation.Run(parameters, rng);

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
    extension(SetTectonicsMobileLid command)
    {
        public Parameters ToParameters(PhysicsC physics, GeometryC geometry) => new()
        {
            BodyAge = physics.Age,
            BodyMass = physics.Mass,
            BodyRadius = geometry.Radius,
            BodySurfaceGravity = physics.Gravity,
            BoundaryFocus = Ratio.FromDecimalFractions(Range10.LinearScale(command.BoundaryFocus, 0, 1)),
            CollisionDominance = Ratio.FromDecimalFractions(Range10.LinearScale(command.CollisionDominance, 0, 1)),
            Grid = GridProvider.Get(geometry.GridShape),
            HotSpotDensity = Ratio.FromDecimalFractions(Range10.LinearScale(command.HotSpotDensity, 0, 1)),
            PlateCount = (int) Math.Round(Range10.LinearScale(command.PlateCount, 4, 50)),
            PlateFragmentation = Ratio.FromDecimalFractions(Range10.LinearScale(command.PlateFragmentation, 0, 1)),
            PlateStability = Ratio.FromDecimalFractions(Range10.LinearScale(command.PlateStability, 0, 1))
        };
    }

    extension(TectonicsCell cell)
    {
        public TectonicsMobileLidC ToComponent() => new()
        {
            PlateSeedCellId = cell.PlateSeedCellId,
            BoundaryType = cell.BoundaryType,
            CrustComposition = cell.CrustComposition,
            CrustalThickness = cell.CrustThickness,
            SeedCellId = cell.SeedCellId,
            VerticalDisplacementRate = cell.VerticalDisplacementRate
        };
    }
}
