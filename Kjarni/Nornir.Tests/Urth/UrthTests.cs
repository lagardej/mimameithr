using Xunit;

namespace Kjarni.Nornir.Tests.Urth;

public sealed class UrðrTests
{
    [Fact]
    public void Configure_StoresAllComponents_OnEntity()
    {
        // var urðr = new Urðr(new EntityStore());
        // var id = urðr.CreateEntity();

        // urðr.Handle(id, new SetGeometry(6_371, 23));
        // urðr.Handle(id, new SetPhysics(5_972, 4_500));
        // urðr.Handle(id, new SetRotation(24));
        // urðr.Handle(id, new SetOrbit(150, 2, 365, 90));
        // urðr.Handle(id, new SetLuminosity(1_000));
        //
        // var geometry = urðr.GetComponent<GeometryC>(id);
        // AssertEx.Close(6_371, geometry.Radius.Kilometers);
        // AssertEx.Close(23, geometry.AxialTilt.Degrees);
        //
        // var physics = urðr.GetComponent<BodyPhysicsSettingsC>(id);
        // AssertEx.Close(5_972e21, physics.Mass.Kilograms, 1e18);
        // AssertEx.Close(4_500e6 * 365.25 * 24 * 3600, physics.Age.Seconds, 1e6);
        //
        // var rotation = urðr.GetComponent<BodyRotationSettingsC>(id);
        // AssertEx.Close(360.0 / (24 * 3600.0), rotation.RotationRate.DegreesPerSecond);
        //
        // var orbit = urðr.GetComponent<OrbitC>(id);
        // AssertEx.Close(150e6, orbit.SemiMajorAxis.Kilometers, 1e-3);
        // AssertEx.Close(2 / 100.0, orbit.Eccentricity);
        // AssertEx.Close(365 * 86400.0, orbit.OrbitalPeriod.Seconds);
        // AssertEx.Close(90, orbit.InitialMeanAnomaly.Degrees);
        //
        // var luminosity = urðr.GetComponent<StellarLuminositySettingsC>(id);
        // AssertEx.Close(3.828e26, luminosity.Luminosity.Watts, 1e20);
    }

    [Fact]
    public void Configure_ThrowsValidationException_WhenOutOfRange()
    {
        // var urth = new Urðr(new EntityStore());
        // var id = urth.CreateEntity();

        // Assert.Throws<ValidationException>(() =>
        //     urth.Handle(id, new SetGeometry(100, 23)));
        //
        // Assert.Throws<ValidationException>(() =>
        //     urth.Handle(id, new SetGeometry(6_371, 200)));
    }
}
