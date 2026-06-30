using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.Autodoc;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Brunnr.Grid;
using Kjarni.Kvasir.Natural.Physical.Physics;
using Kjarni.Nornir.Verthandi.Geimr.BodyGeometry;
using Kjarni.Nornir.Verthandi.Geimr.BodyRotation;
using Kjarni.Nornir.Verthandi.Geimr.Orbit;
using Kjarni.Nornir.Verthandi.Geimr.StellarLuminosity;
using UnitsNet;

namespace Kjarni.Nornir.Verthandi.Eldr.Irradiance;

/// <summary>Solar irradiance state at a surface cell.</summary>
[ComponentKey("irradiance")]
[Component("Eldr/Irradiance")]
public struct IrradianceC : IComponent
{
    /// <summary>Incoming solar flux at this cell's surface.</summary>
    /// <remarks>Computed from stellar luminosity, orbital distance, and solar zenith angle.</remarks>
    [State("W/m²", "Incoming solar flux at this cell's surface.")]
    public UnitsNet.Irradiance Insolation;

    /// <summary>Angle between the sun and the local vertical at this cell. Zero at solar noon directly below the sun.</summary>
    [State("°", "Angle between the sun and the local vertical at this cell.")]
    public Angle SolarZenithAngle;

    /// <summary>Whether this cell is currently on the sunlit side of the body.</summary>
    [State("-", "Whether this cell is currently on the sunlit side of the body.")]
    public bool IsDaytime;
}

/// <summary>
///     Computes <see cref="IrradianceC" /> for each cell entity from planet state via <see cref="CellParentRefC" />.
/// </summary>
public sealed class IrradianceSystem : QuerySystem<IrradianceC, CellIdentityC, CellParentRefC>
{
    /// <inheritdoc />
    protected override void OnUpdate() =>
        Query.ForEachEntity((ref irradiance, ref identity, ref planetRef, _) =>
        {
            var planet = planetRef.Parent;
            var orbit = planet.GetComponent<OrbitC>();
            var rotation = planet.GetComponent<BodyRotationC>();
            var geometry = planet.GetComponent<BodyGeometryC>();
            var luminosity = planet.GetComponent<StellarLuminosityC>();

            var zenith = SolarGeometry.SolarZenithAngle(
                GeoGrid.Instance.CenterOf(identity.Id),
                orbit.OrbitalAngle,
                rotation.CurrentAngle,
                geometry.AxialTilt);

            irradiance.SolarZenithAngle = zenith;
            irradiance.IsDaytime = zenith.Degrees < 90.0;
            irradiance.Insolation = SolarGeometry.Insolation(luminosity.Luminosity, orbit.DistanceFromStar, zenith);
        });
}
