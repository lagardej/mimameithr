using Kjarni.Brunnr.Engine;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Geimr.Position;
using Xunit;

namespace Kjarni.Nornir.Tests;

/// <summary>Tests orbit derivation from position state vectors.</summary>
public class OrbitDerivationTests
{
    [Fact]
    public void DeriveOrbit_EarthAroundSun_Succeeds()
    {
        var engine = new BrunnrEngine();
        var nornir = new Nornir(engine.Store);

        // Create Sun
        var sun = nornir.CreateEntity();
        nornir.Handle(new SetPhysics(sun, Age: 1000, Mass: 1000));
        nornir.Handle(new SetPosition(sun, X: 0, Y: 0, Z: 0, VelocityX: 0, VelocityY: 0, VelocityZ: 0));

        // Create Earth at 150M km with 30 km/s orbital velocity
        var earth = nornir.CreateEntity();
        nornir.Handle(new SetPhysics(earth, Age: 1000, Mass: 600));
        nornir.Handle(new SetPosition(earth, X: 150_000_000, Y: 0, Z: 0, VelocityX: 0, VelocityY: 30, VelocityZ: 0, ParentId: sun));

        // Verify Earth has an orbit component
        var earthEntity = engine.Store.GetEntityById(earth);
        Assert.True(earthEntity.HasComponent<OrbitC>(), "Earth should have OrbitC");
    }

    [Fact]
    public void DeriveOrbit_MoonAroundEarth_Succeeds()
    {
        var engine = new BrunnrEngine();
        var nornir = new Nornir(engine.Store);

        // Create Sun
        var sun = nornir.CreateEntity();
        nornir.Handle(new SetPhysics(sun, Age: 1000, Mass: 1000));
        nornir.Handle(new SetPosition(sun, X: 0, Y: 0, Z: 0, VelocityX: 0, VelocityY: 0, VelocityZ: 0));

        // Create Earth at 150M km with 30 km/s orbital velocity
        var earth = nornir.CreateEntity();
        nornir.Handle(new SetPhysics(earth, Age: 1000, Mass: 600));
        nornir.Handle(new SetPosition(earth, X: 150_000_000, Y: 0, Z: 0, VelocityX: 0, VelocityY: 30, VelocityZ: 0, ParentId: sun));

        // Create Moon at 150M+384k km (absolute) with relative velocity 1.022 km/s
        var moon = nornir.CreateEntity();
        nornir.Handle(new SetPhysics(moon, Age: 1000, Mass: 73));
        nornir.Handle(new SetPosition(moon, X: 150_000_000 + 384_400, Y: 0, Z: 0, VelocityX: 0, VelocityY: 1.022, VelocityZ: 0, ParentId: earth));

        // Verify Moon has an orbit component
        var moonEntity = engine.Store.GetEntityById(moon);
        Assert.True(moonEntity.HasComponent<OrbitC>(), "Moon should have OrbitC");

        var orbit = moonEntity.GetComponent<OrbitC>();
        Assert.True(orbit.Eccentricity is >= 0 and < 1, "Moon orbit should be elliptical");
        Assert.True(orbit.SemiMajorAxis.Kilometers > 0, "Moon semi-major axis should be positive");
    }
}
