using Kvasir;
using Kvasir.Grid;
using Nornir.Hlothyn.Tectonics.MobileLid;
using UnitsNet;

namespace Nornir.Hlothyn.Orogeny;

/// <summary>
///     One-shot orogeny world-generation simulation.
///     Produces an <see cref="OrogenyCell" /> for every cell belonging to a convergent-boundary belt.
/// </summary>
/// <remarks>
///     <para>
///         Convergent cells are grouped into belts by grid adjacency (flood fill). Each belt shares a single
///         orogenic onset time, sampled once and applied to every cell in the belt — a mountain range has one age,
///         not one age per cell.
///     </para>
///     <para>
///         Belt grouping is spatial only: it does not yet distinguish which two plates form a given boundary.
///         Adjacent convergent cells belonging to unrelated plate pairs are currently merged into the same belt.
///         Revisit once <c>Tectonics</c> classifies boundary type per plate pair instead of per cell.
///     </para>
/// </remarks>
public static class OrogenySimulation
{
    /// <summary>Runs the orogeny simulation and returns the per-cell orogenic state.</summary>
    /// <param name="parameters">Simulation parameters.</param>
    /// <param name="tectonics">Per-cell tectonic state, keyed by cell identifier.</param>
    /// <param name="rng">Deterministic random stream.</param>
    /// <returns>Orogenic state for every cell belonging to a convergent-boundary belt.</returns>
    public static Result Run(Parameters parameters, IReadOnlyDictionary<CellId, TectonicsBoundaryC> tectonics,
        StableRandom rng)
    {
        var convergentCells = tectonics
            .Where(kv => kv.Value.BoundaryType == BoundaryType.Convergent)
            .Select(kv => kv.Key)
            .ToHashSet();

        var belts = GroupIntoBelts(convergentCells, parameters.Grid);
        var result = new Dictionary<CellId, OrogenyCell>();

        foreach (var belt in belts)
        {
            var onset = SampleOnset(parameters.BodyAge, parameters.AgeBiasExponent, rng);
            var age = parameters.BodyAge - onset;
            if (age < Duration.Zero)
            {
                age = Duration.Zero;
            }

            var relief = ReliefPotential(age, parameters.ReliefHalfLife);

            foreach (var cellId in belt)
            {
                var rate = tectonics[cellId].VerticalDisplacementRate;
                var shortening = Length.FromMeters(Math.Abs(rate.MetersPerSecond) * age.Seconds);

                result[cellId] = new OrogenyCell
                {
                    OrogenicAge = age,
                    AccumulatedCrustalShortening = shortening,
                    ReliefPotential = relief
                };
            }
        }

        return new Result(result);
    }

    /// <summary>Groups convergent cells into belts via grid-adjacency flood fill.</summary>
    private static List<List<CellId>> GroupIntoBelts(HashSet<CellId> convergentCells, IGrid grid)
    {
        var visited = new HashSet<CellId>();
        var belts = new List<List<CellId>>();

        foreach (var start in convergentCells)
        {
            if (!visited.Add(start))
            {
                continue;
            }

            var belt = new List<CellId> { start };
            var frontier = new Queue<CellId>();
            frontier.Enqueue(start);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                foreach (var neighbour in grid.Disk(current, 1))
                {
                    if (neighbour == current || !convergentCells.Contains(neighbour) || !visited.Add(neighbour))
                    {
                        continue;
                    }

                    belt.Add(neighbour);
                    frontier.Enqueue(neighbour);
                }
            }

            belts.Add(belt);
        }

        return belts;
    }

    /// <summary>Samples a belt's orogenic onset time, skewed by <paramref name="ageBiasExponent" />.</summary>
    private static Duration SampleOnset(Duration bodyAge, double ageBiasExponent, StableRandom rng)
    {
        var skewed = Math.Pow(rng.NextDouble(), ageBiasExponent);
        return Duration.FromJulianYears(bodyAge.JulianYears * skewed);
    }

    /// <summary>Exponential decay of relief potential with orogenic age.</summary>
    private static Ratio ReliefPotential(Duration age, Duration halfLife) =>
        Ratio.FromDecimalFractions(Math.Exp(-age.JulianYears / halfLife.JulianYears));
}
