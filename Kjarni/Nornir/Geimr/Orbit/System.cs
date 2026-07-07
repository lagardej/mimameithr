using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Orbit;

/// <summary>
///     Updates <see cref="OrbitC.MeanAnomaly" />, <see cref="OrbitC.OrbitalAngle" />, and
///     <see cref="OrbitC.DistanceFromStar" />.
/// </summary>
public sealed class OrbitSystem : QuerySystem<OrbitC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.time);
        Query.ForEachEntity((ref orbit, _) =>
        {
            orbit.MeanAnomaly = Kepler.CurrentMeanAnomaly(orbit.MeanAnomaly, orbit.OrbitalPeriod, elapsed);
            orbit.OrbitalAngle = Kepler.MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
            orbit.DistanceFromStar =
                Kepler.RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);
        });
    }
}
