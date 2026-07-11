using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Position;

/// <summary>
///     Updates <see cref="OrbitC.MeanAnomaly" />, <see cref="OrbitC.OrbitalAngle" />,
///     <see cref="OrbitC.DistanceFromStar" />, and <see cref="PositionC" /> for every orbiting body.
/// </summary>
public sealed class PositionSystem : QuerySystem<OrbitC, OrbitParentC, PositionC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.time);
        Query.ForEachEntity((ref orbit, ref parentRef, ref position, _) =>
        {
            orbit.MeanAnomaly = OrbitalMechanics.CurrentMeanAnomaly(orbit.MeanAnomaly, orbit.OrbitalPeriod, elapsed);
            orbit.OrbitalAngle = OrbitalMechanics.MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
            orbit.DistanceFromStar = OrbitalMechanics.RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);

            // Parent is read mid-tick: if the parent itself orbits something and updates later in this same
            // pass, this reads last tick's parent position — one tick of lag for orbit-of-orbits (moons).
            // Acceptable for now; revisit if multi-level orbits need tighter sync.
            var parentPosition = parentRef.Parent.GetComponent<PositionC>();
            var offset = OrbitalMechanics.OrbitalOffset(orbit.DistanceFromStar, orbit.OrbitalAngle, orbit.PeriapsisDirection, orbit.OrbitNormal);

            position.X = parentPosition.X + Length.FromMeters(offset.X);
            position.Y = parentPosition.Y + Length.FromMeters(offset.Y);
            position.Z = parentPosition.Z + Length.FromMeters(offset.Z);
        });
    }
}
