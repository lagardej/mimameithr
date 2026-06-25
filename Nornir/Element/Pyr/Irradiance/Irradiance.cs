using Brunnr.Grid;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Physics;
using Nornir.Element.Aither.BodyGeometry;
using Nornir.Element.Aither.BodyRotation;
using Nornir.Element.Aither.Orbit;
using Nornir.Element.Aither.StellarLuminosity;
using UnitsNet;

namespace Nornir.Element.Pyr.Irradiance;

[ComponentKey("irradiance")]
public struct IrradianceC : IComponent
{
    /// <summary>
    ///     Incoming solar flux at this cell's surface. Derived from stellar luminosity, orbital distance, and solar
    ///     zenith angle.
    /// </summary>
    public UnitsNet.Irradiance Insolation;

    /// <summary>Angle between the sun and the local vertical at this cell. Zero at solar noon directly below the sun.</summary>
    public Angle SolarZenithAngle;

    /// <summary>Whether this cell is currently on the sunlit side of the body.</summary>
    public bool IsDaytime;
}

/// <summary>
///     Computes <see cref="IrradianceC" /> for each cell entity.
///     For each cell, reads planet state via <see cref="PlanetRefC" /> and derives:
///     - Solar flux at the planet's distance (inverse square law).
///     - Solar zenith angle at the cell's geographic position.
///     - Daytime flag from zenith angle (day if angle &lt; 90°).
///     - Insolation as flux scaled by Lambert cosine factor.
/// </summary>
public sealed class IrradianceSystem : QuerySystem<IrradianceC, CellIdentityC, PlanetRefC>
{
    protected override void OnUpdate() =>
        Query.ForEachEntity((ref irradiance, ref identity, ref planetRef, _) =>
        {
            irradiance = ComputeIrradiance(irradiance, identity, planetRef.Entity);
        });

    private static IrradianceC ComputeIrradiance(IrradianceC irradiance, CellIdentityC cellId, Entity planet)
    {
        var orbit = planet.GetComponent<OrbitC>();
        var rotation = planet.GetComponent<BodyRotationC>();
        var geometry = planet.GetComponent<BodyGeometryC>();
        var luminosity = planet.GetComponent<StellarLuminosityC>();

        // Flux at planet distance: F = L / (4π r²)
        var distanceMeters = orbit.DistanceFromStar.Meters;
        var fluxWattsPerM2 = luminosity.Luminosity.Watts / (4.0 * Math.PI * distanceMeters * distanceMeters);

        var zenith = SolarGeometry.SolarZenithAngle(
            GeoGrid.Instance.CenterOf(cellId.Id),
            orbit.OrbitalAngle,
            rotation.CurrentAngle,
            geometry.AxialTilt);

        var isDaytime = zenith.Degrees < 90.0;
        var insolation = isDaytime ? fluxWattsPerM2 * Math.Cos(zenith.Radians) : 0.0;

        irradiance.SolarZenithAngle = zenith;
        irradiance.IsDaytime = isDaytime;
        irradiance.Insolation = UnitsNet.Irradiance.FromWattsPerSquareMeter(insolation);

        return irradiance;
    }
}
