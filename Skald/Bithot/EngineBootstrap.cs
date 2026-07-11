using Kjarni.Brunnr.Grid;
using Kjarni.Nornir;
using Kjarni.Nornir.Geimr.Geometry;
using Kjarni.Nornir.Geimr.Physics;
using Kjarni.Nornir.Geimr.Rotation;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;

namespace Skald.Bithot;

/// <summary>
///     Boots a headless <see cref="Nornir" /> engine and configures a single test body through generation-phase
///     commands.
/// </summary>
public static class NornirBootstrap
{
    private const uint AxialTilt = 23;
    private const uint BodyRadius = 400;
    private const uint BodyAge = 400;
    private const uint BodyMass = 400;
    private const uint RotationPeriod = 400;

    /// <summary>
    ///     Initializes <see cref="GridProvider" /> with the geodesic grid backend, creates one body entity, and runs
    ///     mobile-lid tectonics generation on it. Returns the engine and the body's entity id.
    /// </summary>
    public static int CreateTestBody(Nornir engine)
    {
        var bodyId = engine.CreateEntity();
        engine.Handle(new SetGeometry(bodyId, AxialTilt, GridShape.Spherical, BodyRadius));
        engine.Handle(new SetPhysics(bodyId, BodyAge, BodyMass));
        engine.Handle(new SetRotation(bodyId, 1, RotationPeriod));
        engine.Handle(new SetTectonicsMobileLid(
            bodyId,
            5,
            5,
            5,
            5,
            5,
            5));

        return bodyId;
    }
}