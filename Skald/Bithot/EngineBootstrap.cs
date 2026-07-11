using Kjarni.Nornir;
using Kjarni.Nornir.Eldr.Luminosity;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Geimr.Position;
using Kjarni.Nornir.Geimr.Rotation;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;
using static Kjarni.Brunnr.Grid.GridShape;
using static Skald.Bithot.BootstrapScales;

namespace Skald.Bithot;

/// <summary>
///     Boots a headless <see cref="Nornir" /> engine and configures space bodies through generation-phase
///     commands.
/// </summary>
public static class NornirBootstrap
{
    /// <summary>
    ///     Creates and positions the Sun, Earth, and Moon bodies. Returns a record with their entity ids.
    /// </summary>
    public static void CreateSolarSystem(Nornir engine)
    {
        var sun = CreateSun(engine);
        var earth = CreateEarth(engine, sun);
        CreateMoon(engine, earth);
    }

    private static int CreateSun(Nornir engine)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetGeometry(id, Spherical, RadiusKm(696_000)));
        engine.Handle(new SetPhysics(id, AgeYears(4.6e9), MassKg(1.989e30)));
        engine.Handle(new SetLuminosity(id, LuminositySolar(1.0)));
        engine.Handle(new SetRotation(id, InitialAngle: 1, RotationPeriodDays(25)));
        engine.Handle(new SetPosition(id));
        return id;
    }

    private static int CreateEarth(Nornir engine, int sunId)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetGeometry(id, Spherical, RadiusKm(6_371), AxialTilt: 23));
        engine.Handle(new SetPhysics(id, AgeYears(4.54e9), MassKg(5.972e24)));
        engine.Handle(new SetRotation(id, InitialAngle: 1, RotationPeriodDays(1)));
        engine.Handle(new SetPosition(id, X: 150_000_000, VelocityY: 30, ParentId: sunId));
        return id;
    }

    private static void CreateMoon(Nornir engine, int earthId)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetGeometry(id, Spherical, RadiusKm(1_737)));
        engine.Handle(new SetPhysics(id, AgeYears(4.54e9), MassKg(7.342e22)));
        engine.Handle(new SetRotation(id, InitialAngle: 1, RotationPeriodDays(27.3)));
        engine.Handle(new SetPosition(id, X: 384_400, VelocityY: 1, ParentId: earthId));
    }

    /// <summary>
    ///     Legacy test body creation. Creates one body with tectonics generation.
    /// </summary>
    public static int CreateTestBody(Nornir engine)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetTectonicsMobileLid(id, 5, 5, 5, 5, 5, 5));

        return id;
    }
}
