using Friflo.Engine.ECS.Systems;
using Kjarni.Kvasir.Geimr;
using Kjarni.Nornir.Wyrd.Geimr;
using UnitsNet;

namespace Kjarni.Nornir.Verthandi.Geimr;

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
            orbit.MeanAnomaly = KeplerOrbit.CurrentMeanAnomaly(orbit.MeanAnomaly, orbit.OrbitalPeriod, elapsed);
            orbit.OrbitalAngle = KeplerOrbit.MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
            orbit.DistanceFromStar =
                KeplerOrbit.RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);
        });
    }
}
