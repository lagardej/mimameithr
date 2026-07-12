using Brunnr.Cell;
using Brunnr.Grid;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Grid;
using Nornir.Eldr.Irradiance;
using Nornir.Eldr.Luminosity;
using Nornir.Geimr.Geometry;
using Nornir.Geimr.Position;
using Nornir.Geimr.Rotation;
using System.Diagnostics;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;
using static Nornir.Eldr.Irradiance.StellarIrradiance;
using static Nornir.Geimr.Geometry.StellarGeometry;

namespace Nornir.Tests;

/// <summary>
///     Manual perf comparison for <see cref="IrradianceSystem" />'s parallel query conversion.
///     See <c>.kanban/2_Doing/irradiance-albedo-parallel-query.md</c>, sub-task 4.
/// </summary>
/// <remarks>
///     Not a regression test — wall-clock timings are hardware-dependent and single-sample.
///     Run explicitly (<c>dotnet test --filter IrradianceParallelBenchmark</c>) and read the
///     output; there is no pass/fail threshold on the timings themselves.
///     Sequential path re-uses <see cref="StellarGeometry" /> and <see cref="StellarIrradiance" /> directly (same as the
///     production
///     system) so both paths run identical work — only threading differs.
/// </remarks>
public class IrradianceParallelBenchmark(ITestOutputHelper output)
{
    private const int CellsPerBody = 122; // R0

    [Theory]
    [InlineData(10)] // realistic: ~10 bodies today
    [InlineData(100)] // stress: headroom for higher-resolution grids
    public void Compare_Sequential_Vs_Parallel(int bodyCount)
    {
        var cellCount = bodyCount * CellsPerBody;

        var sequential = RunSequential(cellCount);
        var parallel = RunParallel(cellCount);

        output.WriteLine(
            $"bodies={bodyCount}  cells={cellCount}  sequential={sequential.TotalMilliseconds:F3} ms  " +
            $"parallel={parallel.TotalMilliseconds:F3} ms  speedup={sequential.TotalMilliseconds / parallel.TotalMilliseconds:F2}x  " +
            $"cores={Environment.ProcessorCount}");
    }

    private static TimeSpan RunSequential(int cellCount)
    {
        GridProvider.Initialize(GridShape.Spherical, StubGrid.Instance);
        var store = new EntityStore();
        BuildBodies(store, cellCount);
        var query = store.Query<IrradianceC, CellIdentityC, CellParentRefC>();

        RunOnce(query); // warm-up: JIT + first-chunk allocation
        var sw = Stopwatch.StartNew();
        RunOnce(query);
        return sw.Elapsed;
    }

    private static void RunOnce(ArchetypeQuery<IrradianceC, CellIdentityC, CellParentRefC> query)
    {
        foreach (var (irradiances, identities, planetRefs, _) in query.Chunks)
        {
            for (var n = 0; n < irradiances.Length; n++)
            {
                var planet = planetRefs[n].Parent;
                var orbit = planet.GetComponent<OrbitC>();
                var rotation = planet.GetComponent<RotationC>();
                var geometry = planet.GetComponent<GeometryC>();
                var luminosity = planet.GetComponent<LuminosityC>();

                var grid = (IGeodesicGrid) GridProvider.Get(geometry.GridShape);
                var position = grid.CenterOf(identities[n].Id);

                var zenith = StellarZenithAngle(
                    position, orbit.OrbitalAngle, rotation.CurrentAngle, geometry.AxialTilt);

                irradiances[n].ZenithAngle = zenith;
                irradiances[n].IsDaytime = zenith.Degrees < 90.0;
                irradiances[n].Insolation = Insolation(luminosity.Luminosity, orbit.DistanceFromStar, zenith);
            }
        }
    }

    private static TimeSpan RunParallel(int cellCount)
    {
        GridProvider.Initialize(GridShape.Spherical, StubGrid.Instance);
        var runner = new ParallelJobRunner(Environment.ProcessorCount);
        var store = new EntityStore { JobRunner = runner };
        BuildBodies(store, cellCount);

        var root = new SystemRoot(store) { new IrradianceSystem() };
        root.Update(default); // warm-up: builds + runs the cached QueryJob once

        var sw = Stopwatch.StartNew();
        root.Update(default);
        var elapsed = sw.Elapsed;

        runner.Dispose();
        return elapsed;
    }

    /// <summary>Creates <paramref name="cellCount" /> cells split evenly across bodies of <see cref="CellsPerBody" /> each.</summary>
    private static void BuildBodies(EntityStore store, int cellCount)
    {
        var remaining = cellCount;
        while (remaining > 0)
        {
            var planet = store.CreateEntity();
            planet.AddComponent(new GeometryC
            {
                AxialTilt = Angle.FromDegrees(23),
                GridShape = GridShape.Spherical,
                Radius = Length.FromKilometers(6_371)
            });
            planet.AddComponent(new OrbitC
            {
                DistanceFromStar = Length.FromKilometers(150_000_000),
                OrbitalAngle = Angle.FromDegrees(0)
            });
            planet.AddComponent(new RotationC { CurrentAngle = Angle.FromDegrees(0) });
            planet.AddComponent(new LuminosityC { Luminosity = Luminosity.FromWatts(3.828e26) });

            var cellsOnThisBody = Math.Min(CellsPerBody, remaining);
            for (var i = 0; i < cellsOnThisBody; i++)
            {
                var cell = store.CreateEntity();
                cell.AddComponent(new CellIdentityC { Id = new CellId((ulong) i) });
                cell.AddComponent(new CellParentRefC { Parent = planet });
                cell.AddComponent(new IrradianceC());
            }

            remaining -= cellsOnThisBody;
        }
    }
}
