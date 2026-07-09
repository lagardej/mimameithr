using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Kvasir.Geimr;
using Kjarni.Nornir.Hlothyn.Lithosphere;
using System.Numerics;
using UnitsNet;
using static Kjarni.Nornir.Hlothyn.Lithosphere.CrustalPhysics;
using static Kjarni.Nornir.Hlothyn.Tectonics.MobileLid.BoundaryType;
using static Kjarni.Nornir.Hlothyn.Mantle.MantlePhysics;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>
///     One-shot tectonic world-generation simulation.
///     Produces a <see cref="TectonicsCell" /> per grid cell from <see cref="Parameters" />.
/// </summary>
/// <remarks>
///     <para>Two-pass algorithm:</para>
///     <list type="number">
///         <item>
///             <b>R0 — Plate seeding.</b>
///             Each R0 cell becomes a plate seed. Plate count and size distribution parameters
///             merge seeds into fewer, larger plates via weighted aggregation.
///             Each plate is assigned a crust composition and thickness driven by
///             <see cref="Parameters.CollisionDominance" />.
///         </item>
///         <item>
///             <b>R2 — Boundary classification.</b>
///             Each R2 cell samples its R0 ancestor and its neighbours' R0 ancestors.
///             Cells where neighbours belong to a different plate are boundary cells.
///             Boundary type and vertical displacement rate are derived from
///             <see cref="Parameters.CollisionDominance" /> and
///             <see cref="Parameters.HotSpotDensity" />.
///         </item>
///     </list>
/// </remarks>
internal static class Simulation
{
    private static readonly Resolution s_boundaryResolution = new(2);

    /// <summary>Runs the tectonic simulation and returns the per-cell tectonic state.</summary>
    /// <param name="parameters">Simulation parameters.</param>
    /// <returns>Tectonic state for every R2 cell in the grid.</returns>
    public static Result Run(Parameters parameters)
    {
        var grid = parameters.Grid;

        //
        // Pass 1: plate seeding at R0
        //
        var r0Cells = grid.RootCells();
        var plateMap = SeedPlates(r0Cells, parameters);
        var plateComps = AssignCompositions(plateMap, parameters);
        var angularVelocities = AssignVelocities(plateMap, parameters);

        //
        // Pass 2: boundary classification at R2
        //
        var heatFlux = AsthenosphericHeatFlux(parameters.BodyAge, parameters.BodyMass, parameters.BodyRadius);
        var result = new Dictionary<CellId, TectonicsCell>();
        var gravity = Gravitation.Acceleration(parameters.BodyMass, parameters.BodyRadius);

        foreach (var r2Cell in grid.CellsAtResolution(s_boundaryResolution))
        {
            var plate = grid.ParentOf(r2Cell, Resolution.R0);
            var seedCellId = plateMap[plate];
            var crustComposition = plateComps[seedCellId];
            var crustThickness = CrustThickness(crustComposition, gravity, heatFlux);
            var boundaryType = ClassifyBoundary(r2Cell, seedCellId, grid, plateMap, angularVelocities, parameters);
            var verticalDisplacement =
                VerticalDisplacementRate(boundaryType, crustComposition, crustThickness, parameters, heatFlux);

            result[r2Cell] = new TectonicsCell
            {
                PlateSeedCellId = seedCellId,
                SeedCellId = seedCellId,
                CrustComposition = crustComposition,
                CrustThickness = crustThickness,
                BoundaryType = boundaryType,
                PlateAngularVelocity = angularVelocities[seedCellId],
                VerticalDisplacementRate = verticalDisplacement
            };
        }

        return new Result(result);
    }

    /// <summary>
    ///     Maps each R0 cell to a plate seed (another R0 CellId) via weighted flood fill.
    ///     Guarantees contiguous plates.
    ///     Plate count drives how many distinct seeds exist.
    ///     Distribution drives size variance via per seed expansion weights.
    /// </summary>
    private static Dictionary<CellId, CellId> SeedPlates(CellId[] r0Cells, Parameters parameters)
    {
        var grid = parameters.Grid;
        var rng = parameters.Rng;
        var totalCells = r0Cells.Length;
        var plateCount = Math.Clamp(parameters.PlateCount, 1, totalCells);
        var seeds = r0Cells.OrderBy(_ => rng.NextDouble()).Take(plateCount).ToArray();

        // Distribution: low = equal weights, high = exponential variance.
        var distributionBias = parameters.PlateFragmentation.DecimalFractions;
        var weights = seeds.ToDictionary(
            seed => seed,
            _ => Math.Pow(rng.NextDouble(), 1.0 - distributionBias + 0.01));

        // Flood fill: each seed expands into unclaimed *grid-adjacent* neighbours only.
        // Priority is cumulative cost from the seed (weighted Dijkstra), not an independent
        // random roll per edge — a cell is claimed by whichever plate has the cheapest *path*
        // to it. Growth is monotonic outward and can't leave gaps that later fill from the
        // wrong side (which produced islands / plate-in-plate artefacts).
        var plateMap = seeds.ToDictionary(seed => seed, seed => seed);
        var frontier = new PriorityQueue<(CellId cell, CellId plate, double cost), double>();

        foreach (var seed in seeds)
        foreach (var neighbour in grid.Disk(seed, 1).Where(c => c != seed))
        {
            var cost = rng.NextDouble() / weights[seed];
            frontier.Enqueue((neighbour, seed, cost), cost);
        }

        // Drain frontier in cost order; skip already-claimed cells; keep growing
        // the claiming plate's border by enqueuing the newly-claimed cell's own neighbours
        // at accumulated cost, so distance from the seed is always reflected in priority.
        while (frontier.Count > 0)
        {
            var (candidate, plate, cost) = frontier.Dequeue();
            if (!plateMap.TryAdd(candidate, plate))
            {
                continue;
            }

            foreach (var neighbour in grid.Disk(candidate, 1).Where(c => c != candidate && !plateMap.ContainsKey(c)))
            {
                var nextCost = cost + (rng.NextDouble() / weights[plate]);
                frontier.Enqueue((neighbour, plate, nextCost), nextCost);
            }
        }

        // Any cells unreachable from every seed (disconnected component) fall back to the closest
        // claimed neighbour if one exists, else the first seed — should be rare on a connected grid.
        foreach (var cell in r0Cells.Where(c => !plateMap.ContainsKey(c)))
        {
            var claimedNeighbour = grid.Disk(cell, 1).FirstOrDefault(n => n != cell && plateMap.ContainsKey(n));
            plateMap[cell] = claimedNeighbour != default ? plateMap[claimedNeighbour] : seeds[0];
        }

        return plateMap;
    }

    /// <summary>
    ///     Assigns a <see cref="CrustComposition" /> to each plate seed.
    ///     High <see cref="Parameters.CollisionDominance" /> biases toward felsic crust —
    ///     buoyant plates crumple rather than subduct, producing continental-style crust.
    /// </summary>
    private static Dictionary<CellId, CrustComposition> AssignCompositions(Dictionary<CellId, CellId> plateMap,
        Parameters parameters)
    {
        var rng = parameters.Rng;

        // CollisionDominance: min = mostly mafic (subducting), max = mostly felsic (colliding).
        var felsicProbability = parameters.CollisionDominance.DecimalFractions;

        return plateMap.Values.Distinct().ToDictionary(
            seed => seed,
            _ => rng.NextDouble() < felsicProbability ? CrustComposition.Felsic : CrustComposition.Mafic);
    }

    /// <summary>
    ///     Assigns a rigid-body angular velocity to each plate seed — the physically correct model for
    ///     motion on a sphere (Euler-pole rotation). Linear velocity at any point on the plate is
    ///     <c>cross(angularVelocity, positionUnitVector)</c>, automatically tangent to the sphere.
    ///     Axis is uniform-random on the sphere; magnitude variance is driven by <see cref="Parameters.PlateStability" /> —
    ///     low stability means plates reorganize/change direction often, modelled here as a wider speed spread.
    /// </summary>
    private static Dictionary<CellId, Vector3> AssignVelocities(Dictionary<CellId, CellId> plateMap,
        Parameters parameters)
    {
        var rng = parameters.Rng;
        var speedVariance = parameters.PlateStability.DecimalFractions;

        return plateMap.Values.Distinct().ToDictionary(
            seed => seed,
            _ =>
            {
                // Uniform-random unit axis (Marsaglia-ish via lat/lng sampling is fine here — no need for
                // strict uniformity given this only drives visual/classification variety, not real statistics).
                var axisLat = (rng.NextDouble() * 180.0) - 90.0;
                var axisLng = (rng.NextDouble() * 360.0) - 180.0;
                var axis = new LatLng(axisLat, axisLng).ToUnitVector();

                // Magnitude: baseline speed jittered by stability-driven variance.
                var speed = 0.3 + (rng.NextDouble() * (0.7 + speedVariance));
                return axis * (float) speed;
            });
    }

    /// <summary>
    ///     Classifies the boundary type of R2 cell from actual relative plate motion (Euler-pole rigid rotation),
    ///     not a random roll. Own and neighbouring plate velocities are evaluated at the same point so their
    ///     difference is a true relative velocity; its component along the own→neighbour tangent direction
    ///     decides convergent (closing) vs divergent (separating), and the perpendicular (along-boundary)
    ///     component decides transform (sliding) when it dominates.
    ///     Interior cells (all neighbours share the same plate) → <see cref="BoundaryType.None" /> or
    ///     <see cref="BoundaryType.HotSpot" />, driven by <see cref="Parameters.HotSpotDensity" />.
    /// </summary>
    private static BoundaryType ClassifyBoundary(CellId r2Cell, CellId ownPlate, IGeodesicGrid grid,
        Dictionary<CellId, CellId> plateMap, Dictionary<CellId, Vector3> angularVelocities, Parameters parameters)
    {
        var rng = parameters.Rng;
        var neighbours = grid.Disk(r2Cell, 1).Where(n => n != r2Cell).ToArray();
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

        var otherPlate = neighbourPlates.First(p => p != ownPlate);
        var otherNeighbourCell = neighbours.First(n => plateMap[grid.ParentOf(n, Resolution.R0)] == otherPlate);

        var ownPos = grid.CenterOf(r2Cell).ToUnitVector();
        var neighbourPos = grid.CenterOf(otherNeighbourCell).ToUnitVector();
        var normal = ownPos; // outward sphere normal at r2Cell (unit sphere).

        // Direction from own cell toward the neighbouring plate, projected onto the local tangent plane.
        var toNeighbour = neighbourPos - ownPos;
        toNeighbour -= Vector3.Dot(toNeighbour, normal) * normal;
        if (toNeighbour.LengthSquared() < 1e-12f)
        {
            // Degenerate (neighbour essentially antipodal on the tangent plane) — fall back to Transform.
            return Transform;
        }

        var boundaryDir = Vector3.Normalize(toNeighbour);

        // Evaluate both plates' rigid-rotation velocity at the SAME point so the difference is a true
        // relative velocity, not an artefact of sampling two different positions.
        var ownVelocity = Vector3.Cross(angularVelocities[ownPlate], ownPos);
        var neighbourVelocity = Vector3.Cross(angularVelocities[otherPlate], ownPos);

        // Rate of change of separation along boundaryDir: positive = plates pulling apart (Divergent),
        // negative = plates closing in on each other (Convergent).
        var separationRate = Vector3.Dot(neighbourVelocity - ownVelocity, boundaryDir);

        // Component of relative motion perpendicular to boundaryDir but still in the tangent plane —
        // sliding along the boundary rather than toward/away from it.
        var relativeVelocity = neighbourVelocity - ownVelocity;
        var lateralVelocity = relativeVelocity - (separationRate * boundaryDir);
        var lateralRate = lateralVelocity.Length();

        // CollisionDominance nudges the decision threshold: higher bias means a given closing rate is
        // more readily called Convergent (mirrors the old dial's intent without reverting to a pure roll).
        var dominanceBias = (float) (parameters.CollisionDominance.DecimalFractions - 0.5) * 0.5f;

        if (MathF.Abs(separationRate) + (dominanceBias * MathF.Sign(-separationRate)) > lateralRate)
        {
            return separationRate < 0f ? Convergent : Divergent;
        }

        return Transform;
    }

    /// <summary>
    ///     Derives vertical displacement rate from boundary type, composition, and physical body properties.
    ///     Magnitude is derived from asthenospheric heat flux, crustal density, gravity, and crustal thickness.
    ///     Sign is determined by boundary type and composition.
    ///     <see cref="Parameters.CollisionDominance" /> and <see cref="Parameters.PlateStability" />
    ///     act as pure dimensionless multipliers on magnitude.
    /// </summary>
    private static Speed VerticalDisplacementRate(
        BoundaryType boundaryType,
        CrustComposition composition,
        Length crustalThickness,
        Parameters parameters,
        HeatFlux heatFlux)
    {
        // Crustal density by composition (kg/m³): felsic is buoyant, mafic is dense.
        var density = composition.Density();

        // Convective velocity proxy: heatFlux / (density × gravity × thickness) [m/s].
        // Represents the rate at which mantle convection can drive vertical crustal motion.
        var gravity = Gravitation.Acceleration(parameters.BodyMass, parameters.BodyRadius);
        var baseRate = heatFlux.WattsPerSquareMeter /
                       (density * gravity.MetersPerSecondSquared * crustalThickness.Meters);

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
