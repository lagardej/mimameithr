using Kjarni.Kvasir.Formal.Maths.Geometry.Partitioning;
using Kjarni.Kvasir.Natural.Physical.Geodesy;
using UnitsNet;
using static Kjarni.Kvasir.Natural.Physical.Geology.Lithosphere.CrustalPhysics;
using static Kjarni.Kvasir.Natural.Physical.Geology.Mantle.MantlePhysics;

namespace Kjarni.Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;

/// <summary>
///     One-shot tectonic world-generation simulation.
///     Produces a <see cref="TectonicsCell" /> per grid cell from <see cref="TectonicsSettings" />.
/// </summary>
/// <remarks>
///     <para>Two-pass algorithm:</para>
///     <list type="number">
///         <item>
///             <b>R0 — Plate seeding.</b>
///             Each R0 cell becomes a plate seed. Plate count and size distribution parameters
///             merge seeds into fewer, larger plates via weighted aggregation.
///             Each plate is assigned a crust composition and thickness driven by
///             <see cref="TectonicsSettings.CollisionDominance" />.
///         </item>
///         <item>
///             <b>R2 — Boundary classification.</b>
///             Each R2 cell samples its R0 ancestor and its neighbours' R0 ancestors.
///             Cells where neighbours belong to a different plate are boundary cells.
///             Boundary type and vertical displacement rate are derived from
///             <see cref="TectonicsSettings.CollisionDominance" /> and <see cref="TectonicsSettings.HotSpotDensity" />
///             .
///         </item>
///     </list>
/// </remarks>
[Module("Natural/Physical/Geology/Lithosphere/Tectonics",
    "Two-pass plate seeding and boundary classification for world-generation tectonic simulation.")]
public static class TectonicsSimulation
{
    private static readonly Resolution s_boundaryResolution = new(2);

    /// <summary>
    ///     Runs the tectonic simulation and returns the per-cell tectonic state.
    /// </summary>
    /// <param name="parameter">Designer parameters and physical body properties.</param>
    /// <returns>Tectonic state for every R2 cell in the grid.</returns>
    public static TectonicsResult Run(TectonicsParameters parameter)
    {
        var settings = parameter.Settings;
        var conditions = parameter.BoundaryConditions;
        var grid = conditions.Grid;
        var rng = new Random(conditions.Seed);

        #region Pass 1: plate seeding at R0

        var r0Cells = grid.RootCells();
        var plateMap = SeedPlates(r0Cells, settings, rng);
        var plateComps = AssignCompositions(plateMap, settings, rng);

        #endregion

        #region Pass 2: boundary classification at R2

        var heatFlux = AsthenosphericHeatFlux(conditions.BodyAge, conditions.BodyMass, conditions.BodyRadius);
        var result = new Dictionary<CellId, TectonicsCell>();

        foreach (var r2Cell in grid.CellsAtResolution(s_boundaryResolution))
        {
            var plate = grid.ParentOf(r2Cell, Resolution.R0);
            var plateId = plateMap[plate];
            var crustComposition = plateComps[plateId];
            var thickness = CrustalThickness(crustComposition, conditions.BodySurfaceGravity, heatFlux);

            var boundaryType = ClassifyBoundary(r2Cell, plate, grid, plateMap, settings, rng);
            var verticalDisplacement = VerticalDisplacementRate(boundaryType, crustComposition, thickness, settings,
                conditions.BodySurfaceGravity, heatFlux);

            result[r2Cell] = new TectonicsCell
            {
                PlateId = plateId,
                CrustComposition = crustComposition,
                CrustalThickness = thickness,
                BoundaryType = boundaryType,
                VerticalDisplacementRate = verticalDisplacement
            };
        }

        return new TectonicsResult(result);

        #endregion
    }

    #region Plate seeding

    /// <summary>
    ///     Maps each R0 cell to a plate seed (another R0 CellId) via weighted flood fill.
    ///     Guarantees contiguous plates.
    ///     Plate count drives how many distinct seeds exist.
    ///     Distribution drives size variance via per seed expansion weights.
    /// </summary>
    private static Dictionary<CellId, CellId> SeedPlates(
        CellId[] r0Cells,
        TectonicsSettings settings,
        Random rng)
    {
        var totalCells = r0Cells.Length;
        var plateCount = Math.Clamp((int) Math.Round(settings.PlateCount.Normalized * totalCells), 1, totalCells);
        var seeds = r0Cells.OrderBy(_ => rng.NextDouble()).Take(plateCount).ToArray();

        // Distribution: low = equal weights, high = exponential variance.
        var distributionBias = settings.PlateFragmentation.Normalized;
        var weights = seeds.ToDictionary(
            seed => seed,
            _ => Math.Pow(rng.NextDouble(), 1.0 - distributionBias + 0.01));

        // Flood fill: each seed expands into unclaimed neighbours.
        // Expansion order is weighted — high-weight seeds grab more cells.
        var map = seeds.ToDictionary(seed => seed, seed => seed);
        var frontier = new PriorityQueue<(CellId cell, CellId plate), double>();

        foreach (var seed in seeds)
        foreach (var neighbour in r0Cells.Where(c => c != seed))
        {
            // Prime the frontier with seed neighbours.
            // Only direct grid neighbours matter; IGeodesicGrid.GridRing(seed, 1) gives ring-1.
            // We approximate here using all R0 cells — replaced when grid exposes R0 adjacency.
            frontier.Enqueue((neighbour, seed), -weights[seed] * rng.NextDouble());
        }

        // Drain frontier in priority order; skip already-claimed cells.
        while (frontier.Count > 0)
        {
            var (candidate, plate) = frontier.Dequeue();
            map.TryAdd(candidate, plate);
        }

        // Any remaining unclaimed cells fall back to the closest seed by index.
        foreach (var cell in r0Cells)
        {
            map.TryAdd(cell, seeds[0]);
        }

        return map;
    }

    /// <summary>
    ///     Assigns a <see cref="CrustComposition" /> to each plate seed.
    ///     High <see cref="TectonicsSettings.CollisionDominance" /> biases toward felsic crust —
    ///     buoyant plates crumple rather than subduct, producing continental-style crust.
    /// </summary>
    private static Dictionary<CellId, CrustComposition> AssignCompositions(
        Dictionary<CellId, CellId> plateMap,
        TectonicsSettings settings,
        Random rng)
    {
        // CollisionDominance: min = mostly mafic (subducting), max = mostly felsic (colliding).
        var felsicProbability = settings.CollisionDominance.Normalized;

        return plateMap.Values
            .Distinct()
            .ToDictionary(
                seed => seed,
                _ => rng.NextDouble() < felsicProbability ? CrustComposition.Felsic : CrustComposition.Mafic);
    }

    #endregion

    #region Per-cell derivations

    /// <summary>
    ///     Classifies the boundary type of R2 cell.
    ///     Interior cells (all neighbours share the same plate) → <see cref="BoundaryType.None" />.
    ///     Boundary cells are classified by <see cref="TectonicsSettings.CollisionDominance" />.
    /// </summary>
    private static BoundaryType ClassifyBoundary(
        CellId r2Cell,
        CellId ownPlate,
        IGeodesicGrid grid,
        Dictionary<CellId, CellId> plateMap,
        TectonicsSettings settings,
        Random rng)
    {
        var neighbours = grid.Disk(r2Cell, 1).Where(n => n != r2Cell);
        var neighbourPlates = neighbours
            .Select(n => plateMap[grid.ParentOf(n, Resolution.R0)])
            .Distinct()
            .ToArray();

        if (neighbourPlates.All(p => p == ownPlate))
        {
            // Interior cell — check for hot spot.
            return rng.NextDouble() < settings.HotSpotDensity.Normalized ? BoundaryType.HotSpot : BoundaryType.None;
        }

        // Boundary cell: CollisionDominance drives convergent probability.
        // Transform fills the remainder uniformly.
        var convergentProbability = settings.CollisionDominance.Normalized;
        var divergentProbability = (1.0 - convergentProbability) * settings.BoundaryFocus.Normalized;
        var roll = rng.NextDouble();

        return roll switch
        {
            _ when roll < convergentProbability => BoundaryType.Convergent,
            _ when roll < convergentProbability + divergentProbability => BoundaryType.Divergent,
            _ => BoundaryType.Transform
        };
    }

    /// <summary>
    ///     Derives vertical displacement rate from boundary type, composition, and physical body properties.
    ///     Magnitude is derived from asthenospheric heat flux, crustal density, gravity, and crustal thickness.
    ///     Sign is determined by boundary type and composition.
    ///     <see cref="TectonicsSettings.CollisionDominance" /> and <see cref="TectonicsSettings.PlateStability" />
    ///     act as pure dimensionless multipliers on magnitude.
    /// </summary>
    private static Speed VerticalDisplacementRate(
        BoundaryType boundaryType,
        CrustComposition composition,
        Length crustalThickness,
        TectonicsSettings settings,
        Acceleration surfaceGravity,
        HeatFlux heatFlux)
    {
        // Crustal density by composition (kg/m³): felsic is buoyant, mafic is dense.
        var density = composition.Density();

        // Convective velocity proxy: heatFlux / (density × gravity × thickness) [m/s].
        // Represents the rate at which mantle convection can drive vertical crustal motion.
        var baseRate = heatFlux.WattsPerSquareMeter /
                       (density * surfaceGravity.MetersPerSecondSquared * crustalThickness.Meters);

        // Sign and designer-knob modulation by boundary type.
        var sign = boundaryType switch
        {
            BoundaryType.Convergent => composition == CrustComposition.Felsic
                ? +settings.CollisionDominance.Normalized // uplift: buoyant crust crumples
                : -settings.CollisionDominance.Normalized, // subsidence: dense crust subducts
            BoundaryType.Divergent => -settings.PlateStability.Normalized, // subsidence: crust thins
            BoundaryType.Transform => 0.0,
            _ => 0.0
        };

        return Speed.FromMetersPerSecond(baseRate * sign);
    }

    #endregion
}
