using Friflo.Engine.ECS;
using Nornir.Urth.Geimr.BodyGeometry;
using Nornir.Urth.Geimr.BodyOrbit;
using Nornir.Urth.Geimr.BodyPhysics;
using Nornir.Urth.Geimr.BodyRotation;
using Nornir.Urth.Geimr.StellarLuminosity;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Nornir.Tests.Urth;

public sealed class UrthTests
{
    [Fact]
    public void Configure_StoresAllComponents_OnEntity()
    {
        var urth = new Nornir.Urth.Urth(new EntityStore());
        var id = urth.CreateEntity();

        urth.Configure(id, new BodyGeometry(6_371, 23));
        urth.Configure(id, new ConfigureBodyPhysics(5_972, 4_500));
        urth.Configure(id, new ConfigureBodyRotation(24));
        urth.Configure(id, new ConfigureBodyOrbit(150, 2, 365, 90));
        urth.Configure(id, new ConfigureStellarLuminosity(1_000));

        var geometry = urth.GetComponent<BodyGeometrySettingsC>(id);
        AssertEx.Close(6_371, geometry.Radius.Kilometers);
        AssertEx.Close(23, geometry.AxialTilt.Degrees);

        var physics = urth.GetComponent<BodyPhysicsSettingsC>(id);
        AssertEx.Close(5_972e21, physics.Mass.Kilograms, 1e18);
        AssertEx.Close(4_500e6 * 365.25 * 24 * 3600, physics.Age.Seconds, 1e6);

        var rotation = urth.GetComponent<BodyRotationSettingsC>(id);
        AssertEx.Close(360.0 / (24 * 3600.0), rotation.RotationRate.DegreesPerSecond);

        var orbit = urth.GetComponent<BodyOrbitSettingsC>(id);
        AssertEx.Close(150e6, orbit.SemiMajorAxis.Kilometers, 1e-3);
        AssertEx.Close(2 / 100.0, orbit.Eccentricity);
        AssertEx.Close(365 * 86400.0, orbit.OrbitalPeriod.Seconds);
        AssertEx.Close(90, orbit.InitialMeanAnomaly.Degrees);

        var luminosity = urth.GetComponent<StellarLuminositySettingsC>(id);
        AssertEx.Close(3.828e26, luminosity.Luminosity.Watts, 1e20);
    }

    [Fact]
    public void Configure_ThrowsValidationException_WhenOutOfRange()
    {
        var urth = new Nornir.Urth.Urth(new EntityStore());
        var id = urth.CreateEntity();

        Assert.Throws<ValidationException>(() =>
            urth.Configure(id, new BodyGeometry(100, 23)));

        Assert.Throws<ValidationException>(() =>
            urth.Configure(id, new BodyGeometry(6_371, 200)));
    }
}
