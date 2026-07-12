using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Foundation.Grid;
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
        var rng = randomProvider.CreateStream((ulong) command.Id);

        var parameters = command.ToParameters(entity, rng);
        var result = MobileLidSimulation.Run(parameters);

        RegimeInvariant.RemoveAllRegimeComponents(store, entity);
        entity.AddComponent(new TectonicsRegimeC { Regime = Regime.MobileLid });

        var existingCells = new Dictionary<CellId, Entity>();
        store.Query<CellIdentityC, CellParentRefC>().ForEachEntity((ref identity, ref parentRef, cellEntity) =>
        {
            if (parentRef.Parent == entity)
            {
                existingCells[identity.Id] = cellEntity;
            }
        });

        // R0 — dense: each plate covers many cells; write its component onto every cell it covers, on demand.
        foreach (var plate in result.Plates.Values)
        {
            var component = plate.ToComponent();

            foreach (var cellId in plate.Cells)
            {
                GetOrCreateCell(existingCells, entity, cellId).AddComponent(component);
            }
        }

        // R2 — sparse: only persist boundary/hot-spot cells. The simulation returns every R2 cell (including
        // None); skipping interior cells here is a storage decision, not the simulation's.
        foreach (var (cellId, boundary) in result.Boundaries)
        {
            if (boundary.BoundaryType == BoundaryType.None)
            {
                continue;
            }

            var cellEntity = GetOrCreateCell(existingCells, entity, cellId);
            cellEntity.AddComponent(boundary.ToComponent());
            cellEntity.AddComponent(new TectonicsBoundaryPlateLinkC
            {
                entity = GetOrCreateCell(existingCells, entity, boundary.PlateId)
            });

            if (boundary.OtherPlateId is { } otherPlateId)
            {
                cellEntity.AddComponent(new TectonicsBoundaryOtherPlateLinkC
                {
                    entity = GetOrCreateCell(existingCells, entity, otherPlateId)
                });
            }
        }
    }

    private Entity GetOrCreateCell(Dictionary<CellId, Entity> existingCells, Entity body, CellId cellId)
    {
        if (existingCells.TryGetValue(cellId, out var cellEntity))
        {
            return cellEntity;
        }

        cellEntity = store.CreateEntity();
        cellEntity.AddComponent(new CellIdentityC { Id = cellId });
        cellEntity.AddComponent(new CellParentRefC { Parent = body });
        existingCells[cellId] = cellEntity;
        return cellEntity;
    }
}

internal static class Extensions
{
    private static readonly LinearScale s_linear0To1 = new(Range10, 0, 1);
    private static readonly LinearScale s_plateScale = new(Range10, 4, 5);

    extension(SetTectonicsMobileLid command)
    {
        public Parameters ToParameters(Entity entity, StableRandom rng)
        {
            var geometry = entity.GetComponent<GeometryC>();
            var physics = entity.GetComponent<PhysicsC>();

            return new Parameters
            {
                BodyAge = physics.Age,
                BodyMass = physics.Mass,
                BodyRadius = geometry.Radius,
                BoundaryFocus = Ratio.FromDecimalFractions(s_linear0To1.Evaluate(command.BoundaryFocus)),
                CollisionDominance = Ratio.FromDecimalFractions(s_linear0To1.Evaluate(command.CollisionDominance)),
                Grid = (IGeodesicGrid) GridProvider.Get(geometry.GridShape),
                HotSpotDensity = Ratio.FromDecimalFractions(s_linear0To1.Evaluate(command.HotSpotDensity)),
                PlateCount = (int) Math.Round(s_plateScale.Evaluate(command.PlateCount)),
                PlateFragmentation = Ratio.FromDecimalFractions(s_linear0To1.Evaluate(command.PlateFragmentation)),
                PlateStability = Ratio.FromDecimalFractions(s_linear0To1.Evaluate(command.PlateStability)),
                Rng = rng
            };
        }
    }

    extension(Plate plate)
    {
        public TectonicsPlateC ToComponent() => new()
        {
            CrustComposition = plate.CrustComposition,
            CrustalThickness = plate.CrustThickness,
            PlateAngularVelocity = plate.AngularVelocity,
            PlateSeedCellId = plate.Id
        };
    }

    extension(Boundary boundary)
    {
        public TectonicsBoundaryC ToComponent() => new()
        {
            BoundaryType = boundary.BoundaryType,
            CrustAge = boundary.CrustAge,
            VerticalDisplacementRate = boundary.VerticalDisplacementRate
        };
    }
}
