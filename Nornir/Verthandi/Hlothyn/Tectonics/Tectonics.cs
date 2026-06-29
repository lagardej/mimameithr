using Brunnr.Autodoc;
using Brunnr.Engine.Cell;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir;
using Kvasir.Formal.Maths.Geometry.Partitioning;
using Kvasir.Natural.Physical.Geodesy;
using Kvasir.Natural.Physical.Geology.Lithosphere;
using Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;
using Nornir.Verthandi.Geimr.BodyGeometry;
using Nornir.Verthandi.Geimr.BodyPhysics;
using UnitsNet;

namespace Nornir.Verthandi.Hlothyn.Tectonics;

/// <summary>Designer-facing world-gen knobs for the tectonics simulation.</summary>
[ComponentKey("tectonics-settings")]
[Component("Hlothyn/Tectonics")]
public struct TectonicsSettingsC : IComponent
{
    /// <summary>Number of tectonic plates seeded at world gen. Range: 1–10.</summary>
    [Setting("-", "Number of tectonic plates seeded at world gen.")]
    public Setting10 PlateCount;

    /// <summary>Plate size distribution. Range: 1–10.</summary>
    [Setting("-", "Plate size distribution.")]
    public Setting10 PlateFragmentation;

    /// <summary>Rate at which plates reorganize over time. Range: 1–10.</summary>
    [Setting("-", "Rate at which plates reorganize over time.")]
    public Setting10 PlateStability;

    /// <summary>Degree to which activity concentrates along plate boundaries. Range: 1–10.</summary>
    [Setting("-", "Degree to which activity concentrates along plate boundaries.")]
    public Setting10 BoundaryFocus;

    /// <summary>Bias toward convergent boundaries. Range: 1–10.</summary>
    [Setting("-", "Bias toward convergent boundaries.")]
    public Setting10 CollisionDominance;

    /// <summary>Number and intensity of intraplate volcanic plumes. Range: 1–10.</summary>
    [Setting("-", "Number and intensity of intraplate volcanic plumes.")]
    public Setting10 HotSpotDensity;
}

/// <summary>Tectonic state of a surface cell.</summary>
[ComponentKey("tectonics-state")]
[Component("Hlothyn/Tectonics")]
public struct TectonicsC : IComponent
{
    /// <summary>Dominant crust composition at this cell.</summary>
    [State("-", "Dominant crust composition at this cell.")]
    public CrustComposition CrustComposition;

    /// <summary>Thickness of the crust at this cell.</summary>
    [State("m", "Thickness of the crust at this cell.")]
    public Length CrustalThickness;

    /// <summary>Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.</summary>
    [State("m/s", "Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.")]
    public Speed VerticalDisplacementRate;

    /// <summary>Tectonic boundary type at this cell.</summary>
    [State("-", "Tectonic boundary type at this cell.")]
    public BoundaryType BoundaryType;
}

/// <summary>
///     Computes <see cref="TectonicsC" /> for each cell entity.
///     One-shot at world gen; only re-runs on explicit forcing.
/// </summary>
public sealed class TectonicsSystem : QuerySystem<TectonicsC, CellIdentityC, CellParentRefC>
{
    private readonly IGeodesicGrid _grid;
    private readonly int _seed;

    /// <param name="grid">Geodesic grid shared across all body entities. Injected until a singleton story is settled.</param>
    /// <param name="seed">World generation seed. Injected until a universe-entity or global seed store exists.</param>
    public TectonicsSystem(IGeodesicGrid grid, int seed)
    {
        _grid = grid;
        _seed = seed;
    }

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        // Group cell entities by parent body to run the simulation once per body.
        var cellsByBody = new Dictionary<Entity, List<(Entity cell, CellId cellId)>>();

        Query.ForEachEntity((ref _, ref identity, ref parentRef, cell) =>
        {
            var parent = parentRef.Parent;
            if (!cellsByBody.TryGetValue(parent, out var list))
            {
                list = [];
                cellsByBody[parent] = list;
            }

            list.Add((cell, identity.Id));
        });

        foreach (var (body, cells) in cellsByBody)
        {
            var settings = body.GetComponent<TectonicsSettingsC>();
            var geometry = body.GetComponent<BodyGeometryC>();
            var physics = body.GetComponent<BodyPhysicsC>();

            var parameters = new TectonicsParameters
            {
                Settings = new TectonicsSettings
                {
                    PlateCount = settings.PlateCount,
                    PlateFragmentation = settings.PlateFragmentation,
                    PlateStability = settings.PlateStability,
                    BoundaryFocus = settings.BoundaryFocus,
                    CollisionDominance = settings.CollisionDominance,
                    HotSpotDensity = settings.HotSpotDensity
                },
                BoundaryConditions = new BoundaryConditions
                {
                    Seed = _seed,
                    Grid = _grid,
                    BodyRadius = geometry.Radius,
                    BodySurfaceGravity = physics.SurfaceGravity,
                    BodyMass = physics.Mass,
                    BodyAge = physics.Age
                }
            };

            var result = TectonicsSimulation.Run(parameters);

            foreach (var (cellEntity, cellId) in cells)
            {
                if (!result.Cells.TryGetValue(cellId, out var tectonicsCell))
                {
                    continue;
                }

                cellEntity.AddComponent(new TectonicsC
                {
                    CrustComposition = tectonicsCell.CrustComposition,
                    CrustalThickness = tectonicsCell.CrustalThickness,
                    VerticalDisplacementRate = tectonicsCell.VerticalDisplacementRate,
                    BoundaryType = tectonicsCell.BoundaryType
                });
            }
        }
    }
}
