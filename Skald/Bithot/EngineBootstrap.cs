using Nornir;
using Nornir.Eldr.Luminosity;
using Nornir.Geimr.Geometry;
using Nornir.Geimr.Physics;
using Nornir.Geimr.Position;
using Nornir.Geimr.Rotation;
using Nornir.Hlothyn.Tectonics.MobileLid;
using static Brunnr.Grid.GridShape;
using static Skald.Bithot.BootstrapScales;

namespace Bithot;

/// <summary>
///     Boots a headless <see cref="Nornir" /> engine and configures space bodies through generation-phase
///     commands.
/// </summary>
public static class NornirBootstrap
{
    /// <summary>
    ///     Creates and positions the Sun, Earth, and Moon bodies. Returns a record with their entity ids.
    /// </summary>
    public static void CreateSolarSystem(Nornir.Nornir engine)
    {
        var sun = CreateSun(engine);
        var earth = CreateEarth(engine, sun);
        CreateMoon(engine, earth);
    }

    private static int CreateSun(Nornir.Nornir engine)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetGeometry(id, Spherical, RadiusKm(696_000)));
        engine.Handle(new SetPhysics(id, AgeYears(4.6e9), MassKg(1.989e30)));
        engine.Handle(new SetLuminosity(id, LuminositySolar(1.0)));
        engine.Handle(new SetRotation(id, 1, RotationPeriodDays(25)));
        engine.Handle(new SetPosition(id));
        return id;
    }

    private static int CreateEarth(Nornir.Nornir engine, int sunId)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetGeometry(id, Spherical, RadiusKm(6_371), 23));
        engine.Handle(new SetPhysics(id, AgeYears(4.54e9), MassKg(5.972e24)));
        engine.Handle(new SetRotation(id, 1, RotationPeriodDays(1)));
        engine.Handle(new SetPosition(id, 150_000_000, VelocityY: 30, ParentId: sunId));
        return id;
    }

    private static void CreateMoon(Nornir.Nornir engine, int earthId)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetGeometry(id, Spherical, RadiusKm(1_737)));
        engine.Handle(new SetPhysics(id, AgeYears(4.54e9), MassKg(7.342e22)));
        engine.Handle(new SetRotation(id, 1, RotationPeriodDays(27.3)));
        engine.Handle(new SetPosition(id, 150_000_000 + 384_400, 0, 0, 0, 1.022, 0, earthId));
    }

    /// <summary>
    ///     Legacy test body creation. Creates one body with tectonics generation.
    /// </summary>
    public static int CreateTestBody(Nornir.Nornir engine)
    {
        var id = engine.CreateEntity();
        engine.Handle(new SetTectonicsMobileLid(id, 5, 5, 5, 5, 5, 5));

        return id;
    }
}