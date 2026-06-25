using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Science.Physics;
using Nornir.Aethersphere.BodyGeometry;
using Nornir.Aethersphere.Orbit;
using Nornir.Aethersphere.Rotation;
using Nornir.Aethersphere.StellarLuminosity;

namespace Nornir.Pyrosphere.Irradiance;

/// <summary>
///     Computes <see cref="IrradianceC" /> for each cell entity.
///     For each cell, reads planet state via <see cref="PlanetRefC" /> and derives:
///     - Solar flux at the planet's distance (inverse square law).
///     - Solar zenith angle at the cell's geographic position.
///     - Daytime flag from zenith angle (day if angle &lt; 90°).
///     - Insolation as flux scaled by Lambert cosine factor.
/// </summary>
public sealed class IrradianceSystem : QuerySystem<IrradianceC, IdentityC, PlanetRefC>
{
    protected override void OnUpdate() =>
        Query.ForEachEntity((ref irradiance, ref identity, ref planetRef, _) =>
        {
            irradiance = ComputeIrradiance(irradiance, identity, planetRef.Entity);
        });

    private static IrradianceC ComputeIrradiance(IrradianceC irradiance, IdentityC identity, Entity planet)
    {
        var orbit = planet.GetComponent<OrbitC>();
        var rotation = planet.GetComponent<RotationC>();
        var geometry = planet.GetComponent<BodyGeometryC>();
        var stellar = planet.GetComponent<StellarLuminosityC>();

        // Flux at planet distance: F = L / (4π r²)
        var distanceMeters = orbit.DistanceFromStar.Meters;
        var fluxWattsPerM2 = stellar.Luminosity.Watts / (4.0 * Math.PI * distanceMeters * distanceMeters);

        var zenith = SolarGeometry.SolarZenithAngle(
            Grid.Instance.CenterOf(identity.CellId),
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
