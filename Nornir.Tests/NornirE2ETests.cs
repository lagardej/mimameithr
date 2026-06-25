using Nornir.Aethersphere.Orbit;
using Nornir.Aethersphere.Rotation;
using UnitsNet;
using Xunit;
using Xunit.Abstractions;

namespace Nornir.Tests;

public sealed class NornirE2ETests(ITestOutputHelper output)
{
    private static int CreateEarthLikePlanet(Nornir engine)
    {
        var planetId = 0;
        engine.Configure(store =>
        {
            var planet = store.CreateEntity();
            planet.AddComponent(new OrbitC
            {
                SemiMajorAxis = Length.FromAstronomicalUnits(1.0),
                Eccentricity = 0.0167,
                OrbitalPeriod = Duration.FromDays(365.25),
                MeanAnomaly = Angle.FromDegrees(0)
            });
            planet.AddComponent(new RotationC
            {
                RotationRate = RotationalSpeed.FromDegreesPerSecond(360.0 / 86400.0),
                CurrentAngle = Angle.FromDegrees(0)
            });
            planetId = planet.Id;
        });
        return planetId;
    }

    /// <summary>
    ///     One 1s tick advances MeanAnomaly by mean motion * 1s.
    ///     Expected = 360 / (365.25 * 86400) ≈ 1.1407955e-5 degrees.
    /// </summary>
    [Fact]
    public void Advance_OneTick_1s_OrbitAdvancesByMeanMotion()
    {
        var engine = new Nornir(StubGrid.Instance);
        var planetId = CreateEarthLikePlanet(engine);

        engine.Advance(1f);

        var orbit = engine.Query<OrbitC>(planetId);
        const double expectedDelta = 360.0 / (365.25 * 86400.0);

        AssertEx.Close(expectedDelta, orbit.MeanAnomaly.Degrees, 1e-10);
    }

    /// <summary>
    ///     Variable deltas sum correctly: 3 ticks of 0.5s, 1s, 2s = 3.5s total.
    ///     Expected MeanAnomaly = mean motion * 3.5.
    /// </summary>
    [Fact]
    public void Advance_VariableDeltas_OrbitAccumulatesCorrectly()
    {
        var engine = new Nornir(StubGrid.Instance);
        var planetId = CreateEarthLikePlanet(engine);

        engine.Advance(0.5f);
        engine.Advance(1.0f);
        engine.Advance(2.0f);

        var orbit = engine.Query<OrbitC>(planetId);
        const double meanMotion = 360.0 / (365.25 * 86400.0);
        const double expectedAngle = meanMotion * 3.5;

        AssertEx.Close(expectedAngle, orbit.MeanAnomaly.Degrees, 1e-10);
    }

    /// <summary>
    ///     Perf log smoke test: 100 ticks of 1/30s (30Hz caller).
    /// </summary>
    [Fact]
    public void Advance_100Ticks_30Hz_PerfLog()
    {
        var engine = new Nornir(StubGrid.Instance);
        var planetId = CreateEarthLikePlanet(engine);

        for (var i = 0; i < 100; i++)
        {
            engine.Advance(1f / 30f);
        }

        output.WriteLine(engine.GetPerfLog());

        // sanity: 100 ticks * (1/30)s each = 100/30 ≈ 3.333s elapsed
        var orbit = engine.Query<OrbitC>(planetId);
        const double meanMotion = 360.0 / (365.25 * 86400.0);
        const double expectedAngle = meanMotion * (100.0 / 30.0);

        AssertEx.Close(expectedAngle, orbit.MeanAnomaly.Degrees, 1e-10);
    }
}
