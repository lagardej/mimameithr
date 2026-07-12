using Brunnr.Cell;
using Brunnr.Grid;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Grid;
using Nornir.Eldr.Luminosity;
using Nornir.Geimr.Geometry;
using Nornir.Geimr.Position;
using Nornir.Geimr.Rotation;
using static Nornir.Geimr.Geometry.StellarGeometry;

namespace Nornir.Eldr.Irradiance;

/// <summary>Updates <see cref="IrradianceC" /> for each cell entity from planet state via <see cref="CellParentRefC" />.</summary>
public sealed class IrradianceSystem : QuerySystem<IrradianceC, CellIdentityC, CellParentRefC>
{
    // Reused across ticks per Friflo docs — avoids GC allocation on repeated RunParallel() calls.
    // Built lazily: Query is not populated until the system is attached to a store.
    private QueryJob? _queryJob;

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        _queryJob ??= Query.ForEach((irradiances, identities, planetRefs, _) =>
        {
            for (var n = 0; n < irradiances.Length; n++)
            {
                var planet = planetRefs[n].Parent;
                var orbit = planet.GetComponent<OrbitC>();
                var rotation = planet.GetComponent<RotationC>();
                var geometry = planet.GetComponent<GeometryC>();
                var luminosity = planet.GetComponent<LuminosityC>();

                var grid = (IGeodesicGrid) GridProvider.Get(geometry.GridShape);
                var position = grid.CenterOf(identities[n].Id);

                var zenith = StellarZenithAngle(
                    position,
                    orbit.OrbitalAngle,
                    rotation.CurrentAngle,
                    geometry.AxialTilt);

                irradiances[n].ZenithAngle = zenith;
                irradiances[n].IsDaytime = zenith.Degrees < 90.0;
                irradiances[n].Insolation =
                    StellarIrradiance.Insolation(luminosity.Luminosity, orbit.DistanceFromStar, zenith);
            }
        });
        _queryJob.RunParallel();
    }
}
