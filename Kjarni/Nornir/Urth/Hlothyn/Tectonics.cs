using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Formal.Maths;
using Kjarni.Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;
using Kjarni.Nornir.Wyrd.Geimr;
using Kjarni.Nornir.Wyrd.Ginnungagap;
using Kjarni.Nornir.Wyrd.Hlothyn;
using System.ComponentModel.DataAnnotations;
using UnitsNet;
using static Kjarni.Kvasir.Formal.Maths.Scaling;

namespace Kjarni.Nornir.Urth.Hlothyn;

/// <summary>Command to configure the tectonics of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="BoundaryFocus">Degree to which activity concentrates along plate boundaries. Range: 1–10.</param>
/// <param name="CollisionDominance">Bias toward convergent boundaries. Range: 1–10.</param>
/// <param name="HotSpotDensity">Number and intensity of intraplate volcanic plumes. Range: 1–10.</param>
/// <param name="PlateCount">Number of tectonic plates seeded at world gen. Range: 1–10.</param>
/// <param name="PlateFragmentation">Plate size distribution. Range: 1–10.</param>
/// <param name="PlateStability">Rate at which plates reorganize over time. Range: 1–10.</param>
public record SetTectonics(
    int Id,
    [Range(1u, 10u)] uint BoundaryFocus,
    [Range(1u, 10u)] uint CollisionDominance,
    [Range(1u, 10u)] uint HotSpotDensity,
    [Range(1u, 10u)] uint PlateCount,
    [Range(1u, 10u)] uint PlateFragmentation,
    [Range(1u, 10u)] uint PlateStability
) : ICommand;

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
            BoundaryFocus = Ratio.FromDecimalFractions(Range10.LinearScale(command.BoundaryFocus, 0, 1)),
            CollisionDominance = Ratio.FromDecimalFractions(Range10.LinearScale(command.CollisionDominance, 0, 1)),
            Grid = GridProvider.Get(geometry.GridShape),
            HotSpotDensity = Ratio.FromDecimalFractions(Range10.LinearScale(command.HotSpotDensity, 0, 1)),
            PlateCount = (int) Math.Round(Range10.LinearScale(command.PlateCount, 4, 50)),
            PlateFragmentation = Ratio.FromDecimalFractions(Range10.LinearScale(command.PlateFragmentation, 0, 1)),
            PlateStability = Ratio.FromDecimalFractions(Range10.LinearScale(command.PlateStability, 0, 1)),
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
