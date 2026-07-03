using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Natural.Physical.Physics;
using Kjarni.Nornir.Wyrd.Eldr;
using Kjarni.Nornir.Wyrd.Geimr;

namespace Kjarni.Nornir.Verthandi.Eldr;

/// <summary>Updates <see cref="IrradianceC" /> for each cell entity from planet state via <see cref="CellParentRefC" />.</summary>
public sealed class IrradianceSystem : QuerySystem<IrradianceC, CellIdentityC, CellParentRefC>
{
    /// <inheritdoc />
    protected override void OnUpdate() =>
        Query.ForEachEntity((ref irradiance, ref identity, ref planetRef, _) =>
        {
            var planet = planetRef.Parent;
            var orbit = planet.GetComponent<OrbitC>();
            var rotation = planet.GetComponent<RotationC>();
            var geometry = planet.GetComponent<GeometryC>();
            var luminosity = planet.GetComponent<LuminosityC>();

            var zenith = SolarGeometry.SolarZenithAngle(
                GeoGrid.Instance.CenterOf(identity.Id),
                orbit.OrbitalAngle,
                rotation.CurrentAngle,
                geometry.AxialTilt);

            irradiance.ZenithAngle = zenith;
            irradiance.IsDaytime = zenith.Degrees < 90.0;
            irradiance.Insolation = SolarGeometry.Insolation(luminosity.Luminosity, orbit.DistanceFromStar, zenith);
        });
}
