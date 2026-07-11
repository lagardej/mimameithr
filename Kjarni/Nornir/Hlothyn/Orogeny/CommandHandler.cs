using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Ginnungagap.Seed;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;
using UnitsNet;
using static Kjarni.Kvasir.Foundation.Scaling;

namespace Kjarni.Nornir.Hlothyn.Orogeny;

/// <summary>Handles <see cref="SetOrogeny" /> commands against the entity store.</summary>
public class SetOrogenyHandler(EntityStore store, RandomProvider randomProvider) : ICommandHandler<SetOrogeny>
{
    private static readonly ExponentialScale s_ageBiasScale = new(Range10, 0.3, 3.0);
    private static readonly ExponentialScale s_reliefDecayRateScale = new(Range10, 10e6, 1e9);

    /// <inheritdoc />
    public void Handle(SetOrogeny command)
    {
        var entity = store.GetEntityById(command.Id);
        var geometry = entity.GetComponent<GeometryC>();
        var physics = entity.GetComponent<PhysicsC>();
        var grid = GridProvider.Get(geometry.GridShape);
        var rng = randomProvider.CreateStream((ulong) command.Id, 1);

        var tectonics = new Dictionary<CellId, TectonicsBoundaryC>();
        var cellEntities = new Dictionary<CellId, Entity>();

        store.Query<CellIdentityC, CellParentRefC, TectonicsBoundaryC>()
            .ForEachEntity((ref identity, ref parentRef, ref cellTectonics, cellEntity) =>
            {
                if (parentRef.Parent != entity)
                {
                    return;
                }

                tectonics[identity.Id] = cellTectonics;
                cellEntities[identity.Id] = cellEntity;
            });

        var parameters = new Parameters
        {
            BodyAge = physics.Age,
            AgeBiasExponent = s_ageBiasScale.Evaluate(command.AgeBias),
            ReliefHalfLife = Duration.FromJulianYears(s_reliefDecayRateScale.Evaluate(command.ReliefDecayRate)),
            Grid = grid
        };

        var result = Simulation.Run(parameters, tectonics, rng);

        foreach (var (cellId, cell) in result.Cells)
        {
            cellEntities[cellId].AddComponent(new OrogenyC
            {
                OrogenicAge = cell.OrogenicAge,
                AccumulatedCrustalShortening = cell.AccumulatedCrustalShortening,
                ReliefPotential = cell.ReliefPotential
            });
        }
    }
}
