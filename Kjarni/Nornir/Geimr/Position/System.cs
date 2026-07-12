using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using UnitsNet;

namespace Kjarni.Nornir.Geimr.Position;

/// <summary>
///     Updates <see cref="OrbitC.MeanAnomaly" />, <see cref="OrbitC.OrbitalAngle" />,
///     <see cref="OrbitC.DistanceFromStar" />, and <see cref="PositionC" /> for every orbiting body.
/// </summary>
public sealed class PositionSystem : QuerySystem<OrbitC, OrbitParentC, PositionC>
{
    /// <summary>Creates a system that updates every orbiting entity.</summary>
    public PositionSystem()
    {
    }

    /// <summary>Creates a system limited to entities tagged with <paramref name="tier" />.</summary>
    /// <param name="tier">Update-cadence tier, e.g. from <see cref="Kjarni.Brunnr.System.UpdateTiering" />.</param>
    public PositionSystem(Tags tier) => Filter.AnyTags(tier);

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var elapsed = Duration.FromSeconds(Tick.deltaTime);
        Query.ForEachEntity((ref orbit, ref parentRef, ref position, _) =>
        {
            orbit.MeanAnomaly = OrbitalMechanics.CurrentMeanAnomaly(orbit.MeanAnomaly, orbit.OrbitalPeriod, elapsed);
            orbit.OrbitalAngle = OrbitalMechanics.MeanToTrueAnomaly(orbit.MeanAnomaly, orbit.Eccentricity);
            orbit.DistanceFromStar =
                OrbitalMechanics.RadiusAtTrueAnomaly(orbit.SemiMajorAxis, orbit.Eccentricity, orbit.OrbitalAngle);

            // Parent is read mid-tick: if the parent itself orbits something and updates later in this same
            // pass, this reads last tick's parent position — one tick of lag for orbit-of-orbits (moons).
            // Acceptable for now; revisit if multi-level orbits need tighter sync.
            var parentPosition = parentRef.Parent.GetComponent<PositionC>();
            var offset = OrbitalMechanics.OrbitalOffset(orbit.DistanceFromStar, orbit.OrbitalAngle,
                orbit.PeriapsisDirection, orbit.OrbitNormal);

            position.X = parentPosition.X + Length.FromMeters(offset.X);
            position.Y = parentPosition.Y + Length.FromMeters(offset.Y);
            position.Z = parentPosition.Z + Length.FromMeters(offset.Z);
        });
    }
}
