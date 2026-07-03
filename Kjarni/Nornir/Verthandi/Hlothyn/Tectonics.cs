using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Nornir.Wyrd.Hlothyn;

namespace Kjarni.Nornir.Verthandi.Hlothyn;

/// <summary>Updates <see cref="TectonicsC" /> for each cell entity.</summary>
public sealed class TectonicsSystem : QuerySystem<TectonicsC, CellIdentityC, CellParentRefC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        // // Group cell entities by parent body to run the simulation once per body.
        // var cellsByBody = new Dictionary<Entity, List<(Entity cell, CellId cellId)>>();
        //
        // Query.ForEachEntity((ref _, ref identity, ref parentRef, cell) =>
        // {
        //     var parent = parentRef.Parent;
        //     if (!cellsByBody.TryGetValue(parent, out var list))
        //     {
        //         list = [];
        //         cellsByBody[parent] = list;
        //     }
        //
        //     list.Add((cell, identity.Id));
        // });
        //
        // foreach (var (body, cells) in cellsByBody)
        // {
        //     var settings = body.GetComponent<TectonicsSettingsC>();
        //     var geometry = body.GetComponent<BodyGeometryC>();
        //     var physics = body.GetComponent<BodyPhysicsC>();
        //
        //     var parameters = new TectonicsParameters
        //     {
        //         Settings = new TectonicsSettings
        //         {
        //             PlateCount = settings.PlateCount,
        //             PlateFragmentation = settings.PlateFragmentation,
        //             PlateStability = settings.PlateStability,
        //             BoundaryFocus = settings.BoundaryFocus,
        //             CollisionDominance = settings.CollisionDominance,
        //             HotSpotDensity = settings.HotSpotDensity
        //         },
        //         BoundaryConditions = new BoundaryConditions
        //         {
        //             Seed = _seed,
        //             Grid = _grid,
        //             BodyRadius = geometry.Radius,
        //             BodySurfaceGravity = physics.SurfaceGravity,
        //             BodyMass = physics.Mass,
        //             BodyAge = physics.Age
        //         }
        //     };
        //
        //     var result = TectonicsSimulation.Run(parameters);
        //
        //     foreach (var (cellEntity, cellId) in cells)
        //     {
        //         if (!result.Cells.TryGetValue(cellId, out var tectonicsCell))
        //         {
        //             continue;
        //         }
        //
        //         cellEntity.AddComponent(new TectonicsC
        //         {
        //             CrustComposition = tectonicsCell.CrustComposition,
        //             CrustalThickness = tectonicsCell.CrustalThickness,
        //             VerticalDisplacementRate = tectonicsCell.VerticalDisplacementRate,
        //             BoundaryType = tectonicsCell.BoundaryType
        //         });
        //     }
        // }
    }
}
