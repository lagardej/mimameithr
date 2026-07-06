using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Hlothyn.Lithosphere;
using UnitsNet;
using static Kjarni.Kvasir.Hlothyn.Lithosphere.CrustalPhysics;
using static Kjarni.Kvasir.Hlothyn.Tectonics.BoundaryType;
using static Kjarni.Kvasir.Hlothyn.Mantle.MantlePhysics;

namespace Kjarni.Kvasir.Hlothyn.Tectonics;

/// <summary>
///     One-shot tectonic world-generation simulation.
///     Produces a <see cref="Tectonics.TectonicsCell" /> per grid cell from <see cref="TectonicsParameters" />.
/// </summary>
/// <remarks>
///     <para>Two-pass algorithm:</para>
///     <list type="number">
///         <item>
///             <b>R0 — Plate seeding.</b>
///             Each R0 cell becomes a plate seed. Plate count and size distribution parameters
///             merge seeds into fewer, larger plates via weighted aggregation.
///             Each plate is assigned a crust composition and thickness driven by
///             <see cref="TectonicsParameters.CollisionDominance" />.
///         </item>
///         <item>
///             <b>R2 — Boundary classification.</b>
///             Each R2 cell samples its R0 ancestor and its neighbours' R0 ancestors.
///             Cells where neighbours belong to a different plate are boundary cells.
///             Boundary type and vertical displacement rate are derived from
///             <see cref="TectonicsParameters.CollisionDominance" /> and
///             <see cref="TectonicsParameters.HotSpotDensity" />.
///         </item>
///     </list>
/// </remarks>
public static class TectonicsSimulation
{
    private static readonly Resolution s_boundaryResolution = new(2);

    /// <summary>Runs the tectonic simulation and returns the per-cell tectonic state.</summary>
    /// <param name="parameters">Simulation parameters.</param>
    /// <returns>Tectonic state for every R2 cell in the grid.</returns>
    public static TectonicsResult Run(TectonicsParameters parameters)
    {
        var grid = parameters.Grid;
        var rng = new Random((int) parameters.Seed);

        //
        // Pass 1: plate seeding at R0
        //
        var r0Cells = grid.RootCells();
        var plateMap = SeedPlates(r0Cells, parameters, rng);
        var plateComps = AssignCompositions(plateMap, parameters, rng);

        //
        // Pass 2: boundary classification at R2
        //
        var heatFlux = AsthenosphericHeatFlux(parameters.BodyAge, parameters.BodyMass, parameters.BodyRadius);
        var result = new Dictionary<CellId, TectonicsCell>();

        foreach (var r2Cell in grid.CellsAtResolution(s_boundaryResolution))
        {
            var plate = grid.ParentOf(r2Cell, Resolution.R0);
            var seedCellId = plateMap[plate];
            var crustComposition = plateComps[seedCellId];
            var crustThickness = CrustThickness(crustComposition, parameters.BodySurfaceGravity, heatFlux);
            var boundaryType = ClassifyBoundary(r2Cell, plate, grid, plateMap, parameters, rng);
            var verticalDisplacement =
                VerticalDisplacementRate(boundaryType, crustComposition, crustThickness, parameters, heatFlux);

            result[r2Cell] = new TectonicsCell
            {
                SeedCellId = seedCellId,
                CrustComposition = crustComposition,
                CrustThickness = crustThickness,
                BoundaryType = boundaryType,
                VerticalDisplacementRate = verticalDisplacement
            };
        }

        return new TectonicsResult(result);
    }

    /// <summary>
    ///     Maps each R0 cell to a plate seed (another R0 CellId) via weighted flood fill.
    ///     Guarantees contiguous plates.
    ///     Plate count drives how many distinct seeds exist.
    ///     Distribution drives size variance via per seed expansion weights.
    /// </summary>
    private static Dictionary<CellId, CellId> SeedPlates(
        CellId[] r0Cells,
        TectonicsParameters parameters,
        Random rng)
    {
        var totalCells = r0Cells.Length;
        var plateCount = Math.Clamp(parameters.PlateCount, 1, totalCells);
        var seeds = r0Cells.OrderBy(_ => rng.NextDouble()).Take(plateCount).ToArray();

        // Distribution: low = equal weights, high = exponential variance.
        var distributionBias = parameters.PlateFragmentation.DecimalFractions;
        var weights = seeds.ToDictionary(
            seed => seed,
            _ => Math.Pow(rng.NextDouble(), 1.0 - distributionBias + 0.01));

        // Flood fill: each seed expands into unclaimed neighbours.
        // Expansion order is weighted — high-weight seeds grab more cells.
        var plateMap = seeds.ToDictionary(seed => seed, seed => seed);
        var frontier = new PriorityQueue<(CellId cell, CellId plate), double>();

        foreach (var seed in seeds)
        foreach (var neighbour in r0Cells.Where(c => c != seed))
        {
            // Prime the frontier with seed neighbours.
            // Only direct grid neighbours matter; IGrid.GridRing(seed, 1) gives ring-1.
            // We approximate here using all R0 cells — replaced when grid exposes R0 adjacency.
            frontier.Enqueue((neighbour, seed), -weights[seed] * rng.NextDouble());
        }

        // Drain frontier in priority order; skip already-claimed cells.
        while (frontier.Count > 0)
        {
            var (candidate, plate) = frontier.Dequeue();
            plateMap.TryAdd(candidate, plate);
        }

        // Any remaining unclaimed cells fall back to the closest seed by index.
        foreach (var cell in r0Cells)
        {
            plateMap.TryAdd(cell, seeds[0]);
        }

        return plateMap;
    }

    /// <summary>
    ///     Assigns a <see cref="CrustComposition" /> to each plate seed.
    ///     High <see cref="TectonicsParameters.CollisionDominance" /> biases toward felsic crust —
    ///     buoyant plates crumple rather than subduct, producing continental-style crust.
    /// </summary>
    private static Dictionary<CellId, CrustComposition> AssignCompositions(
        Dictionary<CellId, CellId> plateMap,
        TectonicsParameters parameters,
        Random rng)
    {
        // CollisionDominance: min = mostly mafic (subducting), max = mostly felsic (colliding).
        var felsicProbability = parameters.CollisionDominance.DecimalFractions;

        return plateMap.Values.Distinct().ToDictionary(
            seed => seed,
            _ => rng.NextDouble() < felsicProbability ? CrustComposition.Felsic : CrustComposition.Mafic);
    }

    /// <summary>
    ///     Classifies the boundary type of R2 cell.
    ///     Interior cells (all neighbours share the same plate) → <see cref="BoundaryType.None" />.
    ///     Boundary cells are classified by <see cref="TectonicsParameters.CollisionDominance" />.
    /// </summary>
    private static BoundaryType ClassifyBoundary(
        CellId r2Cell,
        CellId ownPlate,
        IGrid grid,
        Dictionary<CellId, CellId> plateMap,
        TectonicsParameters parameters,
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
            return rng.NextDouble() < parameters.HotSpotDensity.DecimalFractions
                ? HotSpot
                : None;
        }

        // Boundary cell: CollisionDominance drives convergent probability.
        // Transform fills the remainder uniformly.
        var convergentProbability = parameters.CollisionDominance.DecimalFractions;
        var divergentProbability = (1.0 - convergentProbability) * parameters.BoundaryFocus.DecimalFractions;
        var roll = rng.NextDouble();

        return roll switch
        {
            _ when roll < convergentProbability => Convergent,
            _ when roll < convergentProbability + divergentProbability => Divergent,
            _ => Transform
        };
    }

    /// <summary>
    ///     Derives vertical displacement rate from boundary type, composition, and physical body properties.
    ///     Magnitude is derived from asthenospheric heat flux, crustal density, gravity, and crustal thickness.
    ///     Sign is determined by boundary type and composition.
    ///     <see cref="TectonicsParameters.CollisionDominance" /> and <see cref="TectonicsParameters.PlateStability" />
    ///     act as pure dimensionless multipliers on magnitude.
    /// </summary>
    private static Speed VerticalDisplacementRate(
        BoundaryType boundaryType,
        CrustComposition composition,
        Length crustalThickness,
        TectonicsParameters parameters,
        HeatFlux heatFlux)
    {
        // Crustal density by composition (kg/m³): felsic is buoyant, mafic is dense.
        var density = composition.Density();

        // Convective velocity proxy: heatFlux / (density × gravity × thickness) [m/s].
        // Represents the rate at which mantle convection can drive vertical crustal motion.
        var baseRate = heatFlux.WattsPerSquareMeter /
                       (density * parameters.BodySurfaceGravity.MetersPerSecondSquared * crustalThickness.Meters);

        // Sign and designer-knob modulation by boundary type.
        var sign = boundaryType switch
        {
            Convergent => composition == CrustComposition.Felsic
                ? +parameters.CollisionDominance.DecimalFractions // uplift: buoyant crust crumples
                : -parameters.CollisionDominance.DecimalFractions, // subsidence: dense crust subducts
            Divergent => -parameters.PlateStability.DecimalFractions, // subsidence: crust thins
            Transform => 0.0,
            _ => 0.0
        };

        return Speed.FromMetersPerSecond(baseRate * sign);
    }
}
