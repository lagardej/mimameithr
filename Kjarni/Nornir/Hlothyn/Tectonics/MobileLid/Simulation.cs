using Kjarni.Kvasir.Foundation;
using Kjarni.Kvasir.Foundation.Grid;
using Kjarni.Kvasir.Geimr;
using Kjarni.Nornir.Hlothyn.Lithosphere;
using System.Numerics;
using UnitsNet;
using static Kjarni.Nornir.Hlothyn.Lithosphere.CrustalPhysics;
using static Kjarni.Nornir.Hlothyn.Mantle.MantlePhysics;
using static Kjarni.Nornir.Hlothyn.Tectonics.MobileLid.BoundaryType;

namespace Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

/// <summary>
///     One-shot tectonic world-generation simulation.
///     Produces a <see cref="Plate" /> per plate (covering many R0 cells) and a <see cref="Boundary" /> per R2
///     boundary/hot-spot cell from <see cref="Parameters" />.
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
    /// <returns>Every plate and boundary state for every R2 boundary/hot-spot cell.</returns>
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
        var heatFlux = AsthenosphericHeatFlux(parameters.BodyAge, parameters.BodyMass, parameters.BodyRadius);
        var gravity = Gravitation.Acceleration(parameters.BodyMass, parameters.BodyRadius);

        var plateThickness = plateComps.ToDictionary(
            kv => kv.Key,
            kv => CrustThickness(kv.Value, gravity, heatFlux));

        var plates = plateMap
            .GroupBy(kv => kv.Value)
            .ToDictionary(
                g => g.Key,
                g => new Plate
                {
                    Id = g.Key,
                    CrustComposition = plateComps[g.Key],
                    CrustThickness = plateThickness[g.Key],
                    AngularVelocity = angularVelocities[g.Key],
                    Cells = g.Select(kv => kv.Key).ToArray()
                });

        //
        // Pass 2: boundary classification at R2
        //
        var boundaries = new Dictionary<CellId, Boundary>();
        var r2Composition = new Dictionary<CellId, CrustComposition>();

        foreach (var r2Cell in grid.CellsAtResolution(s_boundaryResolution))
        {
            var plate = grid.ParentOf(r2Cell, Resolution.R0);
            var seedCellId = plateMap[plate];
            var crustComposition = plateComps[seedCellId];
            var crustThickness = plateThickness[seedCellId];
            var (boundaryType, otherPlateId) =
                ClassifyBoundary(r2Cell, seedCellId, grid, plateMap, angularVelocities, parameters);
            var verticalDisplacement =
                VerticalDisplacementRate(boundaryType, crustComposition, crustThickness, parameters, heatFlux);

            r2Composition[r2Cell] = crustComposition;

            boundaries[r2Cell] = new Boundary
            {
                PlateId = seedCellId,
                OtherPlateId = otherPlateId,
                CrustAge = Duration.Zero,
                BoundaryType = boundaryType,
                VerticalDisplacementRate = verticalDisplacement
            };
        }

        //
        // Pass 3: crust age from distance to nearest divergent boundary
        //
        AssignCrustAge(boundaries, r2Composition, grid, parameters.BodyAge);

        return new Result(plates, boundaries);
    }

    /// <summary>
    ///     Approximates crust age via multi-source BFS from divergent-boundary cells, since a one-shot generator
    ///     has no actual crust-creation history to track. Only meaningful for <see cref="CrustComposition.Mafic" />
    ///     crust (oceanic-style spreading); <see cref="CrustComposition.Felsic" /> crust is treated as old/stable,
    ///     matching Earth where continental crust is not recycled by spreading.
    /// </summary>
    private static void AssignCrustAge(Dictionary<CellId, Boundary> boundaries,
        Dictionary<CellId, CrustComposition> composition, IGeodesicGrid grid, Duration bodyAge)
    {
        var distances = new Dictionary<CellId, int>();
        var frontier = new Queue<CellId>();

        foreach (var (cellId, cell) in boundaries)
        {
            if (cell.BoundaryType != Divergent)
            {
                continue;
            }

            distances[cellId] = 0;
            frontier.Enqueue(cellId);
        }

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            var nextDistance = distances[current] + 1;

            foreach (var neighbour in grid.Disk(current, 1).Where(n => n != current))
            {
                if (distances.ContainsKey(neighbour) ||
                    !composition.TryGetValue(neighbour, out var neighbourComposition) ||
                    neighbourComposition != CrustComposition.Mafic)
                {
                    continue;
                }

                distances[neighbour] = nextDistance;
                frontier.Enqueue(neighbour);
            }
        }

        var maxDistance = distances.Count > 0 ? distances.Values.Max() : 0;

        foreach (var cellId in boundaries.Keys.ToArray())
        {
            var cell = boundaries[cellId];

            Duration age;
            if (composition[cellId] == CrustComposition.Felsic)
            {
                age = bodyAge;
            }
            else if (distances.TryGetValue(cellId, out var distance) && maxDistance > 0)
            {
                age = Duration.FromJulianYears(bodyAge.JulianYears * distance / maxDistance);
            }
            else
            {
                // Mafic cell unreachable from any divergent source — treat as oldest.
                age = bodyAge;
            }

            boundaries[cellId] = cell with { CrustAge = age };
        }
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
    private static (BoundaryType Type, CellId? OtherPlate) ClassifyBoundary(CellId r2Cell, CellId ownPlate,
        IGeodesicGrid grid, Dictionary<CellId, CellId> plateMap, Dictionary<CellId, Vector3> angularVelocities,
        Parameters parameters)
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
            return (rng.NextDouble() < parameters.HotSpotDensity.DecimalFractions ? HotSpot : None, null);
        }

        var ownPos = grid.CenterOf(r2Cell).ToUnitVector();
        var normal = ownPos; // outward sphere normal at r2Cell (unit sphere).
        var ownVelocity = Vector3.Cross(angularVelocities[ownPlate], ownPos);

        // At a triple (or higher) junction, several distinct plates border the same cell. Evaluate each
        // candidate independently and keep the one with the strongest relative-motion signal — that
        // interaction dominates the cell's seismic/volcanic character, the others are locally weaker.
        var bestScore = -1f;
        CellId? bestOtherPlate = null;
        var bestSeparationRate = 0f;
        var bestLateralRate = 0f;

        foreach (var otherPlate in neighbourPlates.Where(p => p != ownPlate))
        {
            var otherNeighbourCell = neighbours.First(n => plateMap[grid.ParentOf(n, Resolution.R0)] == otherPlate);
            var neighbourPos = grid.CenterOf(otherNeighbourCell).ToUnitVector();

            // Direction from own cell toward the neighbouring plate, projected onto the local tangent plane.
            var toNeighbour = neighbourPos - ownPos;
            toNeighbour -= Vector3.Dot(toNeighbour, normal) * normal;
            if (toNeighbour.LengthSquared() < 1e-12f)
            {
                // Degenerate (neighbour essentially antipodal on the tangent plane) — skip, can't score it.
                continue;
            }

            var boundaryDir = Vector3.Normalize(toNeighbour);

            // Evaluate both plates' rigid-rotation velocity at the SAME point so the difference is a true
            // relative velocity, not an artefact of sampling two different positions.
            var neighbourVelocity = Vector3.Cross(angularVelocities[otherPlate], ownPos);
            var relativeVelocity = neighbourVelocity - ownVelocity;

            // Rate of change of separation along boundaryDir: positive = plates pulling apart (Divergent),
            // negative = plates closing in on each other (Convergent).
            var separationRate = Vector3.Dot(relativeVelocity, boundaryDir);

            // Component of relative motion perpendicular to boundaryDir but still in the tangent plane —
            // sliding along the boundary rather than toward/away from it.
            var lateralVelocity = relativeVelocity - (separationRate * boundaryDir);
            var lateralRate = lateralVelocity.Length();

            var score = relativeVelocity.Length();
            if (score > bestScore)
            {
                bestScore = score;
                bestOtherPlate = otherPlate;
                bestSeparationRate = separationRate;
                bestLateralRate = lateralRate;
            }
        }

        if (bestOtherPlate is not { } dominantPlate)
        {
            // All candidates degenerate — fall back to Transform against an arbitrary neighbour.
            return (Transform, neighbourPlates.First(p => p != ownPlate));
        }

        // CollisionDominance nudges the decision threshold: higher bias means a given closing rate is
        // more readily called Convergent (mirrors the old dial's intent without reverting to a pure roll).
        var dominanceBias = (float) (parameters.CollisionDominance.DecimalFractions - 0.5) * 0.5f;

        if (MathF.Abs(bestSeparationRate) + (dominanceBias * MathF.Sign(-bestSeparationRate)) > bestLateralRate)
        {
            return (bestSeparationRate < 0f ? Convergent : Divergent, dominantPlate);
        }

        return (Transform, dominantPlate);
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
