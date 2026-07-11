using Friflo.Engine.ECS.Systems;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Kvasir.Foundation;
using System.Numerics;
using UnitsNet;

namespace Kjarni.Nornir.Eldr.Irradiance;

/// <summary>Updates <see cref="IrradianceC" /> for each cell entity from planet state via <see cref="CellParentRefC" />.</summary>
public sealed class IrradianceSystem : QuerySystem<IrradianceC, CellIdentityC, CellParentRefC>
{
    /// <inheritdoc />
    protected override void OnUpdate() =>
        Query.ForEachEntity((ref irradiance, ref identity, ref planetRef, _) =>
        {
            // Blocked on cell-id -> LatLng lookup: IGrid has no such method, GeoGrid referenced here
            // pre-refactor doesn't exist. Orbit/Rotation/Geometry/Luminosity inputs below are otherwise
            // ready (orbit-system-rewrite.md re-derives OrbitC.OrbitalAngle/DistanceFromStar) -- only the
            // geographic lookup is missing. Separate ticket.
            // var planet = planetRef.Parent;
            // var orbit = planet.GetComponent<OrbitC>();
            // var rotation = planet.GetComponent<RotationC>();
            // var geometry = planet.GetComponent<GeometryC>();
            // var luminosity = planet.GetComponent<Luminosity.LuminosityC>();
            //
            // var zenith = StellarZenithAngle(
            //     /* cell centre LatLng lookup -- missing */,
            //     orbit.OrbitalAngle,
            //     rotation.CurrentAngle,
            //     geometry.AxialTilt);
            //
            // irradiance.ZenithAngle = zenith;
            // irradiance.IsDaytime = zenith.Degrees < 90.0;
            // irradiance.Insolation = Insolation(luminosity.Luminosity, orbit.DistanceFromStar, zenith);
        });


    /// <summary>Computes the stellar flux at a surface cell. Returns zero on the night side.</summary>
    /// <param name="luminosity">Total power radiated by the star across all wavelengths.</param>
    /// <param name="distanceFromStar">Distance between the body and its parent star.</param>
    /// <param name="zenithAngle">Stellar zenith angle at the cell.</param>
    /// <returns>Incoming stellar flux at the cell's surface.</returns>
    private static UnitsNet.Irradiance Insolation(UnitsNet.Luminosity luminosity, Length distanceFromStar,
        Angle zenithAngle)
    {
        var fluxWattsPerM2 = luminosity.Watts / (4.0 * Math.PI * distanceFromStar.Meters * distanceFromStar.Meters);
        var watts = zenithAngle.Degrees < 90.0 ? fluxWattsPerM2 * Math.Cos(zenithAngle.Radians) : 0.0;
        return UnitsNet.Irradiance.FromWattsPerSquareMeter(watts);
    }

    /// <summary>
    ///     Computes the stellar zenith angle at a surface cell.
    ///     The star's direction is expressed in the planet's body frame, accounting for axial tilt
    ///     (seasonal effect) and the body's current rotation angle (diurnal effect).
    ///     The cell's surface normal is derived from its geographic position.
    /// </summary>
    /// <param name="position">Cell centre geographic position.</param>
    /// <param name="orbitalAngle">True anomaly — current angular position in the orbit.</param>
    /// <param name="rotationAngle">Current rotation angle of the body around its axis.</param>
    /// <param name="axialTilt">Angle between the body's rotation axis and its orbital plane normal.</param>
    /// <returns>Stellar zenith angle. Values above 90° indicate the night side.</returns>
    private static Angle StellarZenithAngle(LatLng position, Angle orbitalAngle, Angle rotationAngle, Angle axialTilt)
    {
        // Planet → star direction in the orbital plane.
        var starDirectionOrbital = new Vector3((float) -Math.Cos(orbitalAngle.Radians),
            (float) -Math.Sin(orbitalAngle.Radians), 0f);

        // Apply axial tilt (Y) then planetary rotation (Z).
        var starDir = Vector3.Transform(starDirectionOrbital, Matrix4x4.CreateRotationY((float) axialTilt.Radians));
        starDir = Vector3.Transform(starDir, Matrix4x4.CreateRotationZ((float) -rotationAngle.Radians));

        // Outward surface normal at the cell's geographic position.
        var normal = position.ToUnitVector();

        // Zenith angle between the local surface normal and the star direction.
        // cos θ = 1 => star at zenith
        // cos θ = 0 => terminator
        // cos θ < 0 => night side
        var cosTheta = Vector3.Dot(normal, starDir);

        return Angle.FromRadians(Math.Acos(Math.Clamp(cosTheta, -1f, 1f)));
    }
}
