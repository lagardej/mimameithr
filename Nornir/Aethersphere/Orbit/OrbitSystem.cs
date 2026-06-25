using Friflo.Engine.ECS.Systems;
using Kvasir.Science.Physics.OrbitalMechanics;
using UnitsNet;

namespace Nornir.Aethersphere.Orbit;

/// <summary>
///     Advances <see cref="OrbitC.MeanAnomaly" /> by integrating mean motion derived from
///     <see cref="OrbitC.OrbitalPeriod" />, converts to true anomaly via Kepler's equation,
///     then updates <see cref="OrbitC.OrbitalAngle" /> and <see cref="OrbitC.DistanceFromStar" />.
/// </summary>
public sealed class OrbitSystem : QuerySystem<OrbitC>
{
    protected override void OnUpdate()
    {
        var delta = Tick.deltaTime;
        Query.ForEachEntity((ref orbit, _) => orbit = ComputeOrbit(orbit, delta));
    }

    private static OrbitC ComputeOrbit(OrbitC orbit, float delta)
    {
        var meanMotionDeg = 360.0 / orbit.OrbitalPeriod.Seconds;
        orbit.MeanAnomaly = Angle.FromDegrees((orbit.MeanAnomaly.Degrees + (meanMotionDeg * delta)) % 360.0);
        orbit.OrbitalAngle = KeplerOrbit.MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
        orbit.DistanceFromStar =
            KeplerOrbit.RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);

        return orbit;
    }
}
