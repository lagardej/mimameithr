using Kvasir.Science.Geography;
using Kvasir.Science.Geography.Spatial;
using Kvasir.Science.Maths.Geometry;
using Kvasir.Science.Maths.Geometry.Partitioning;
using Kvasir.Science.Physics;
using Nornir.Component.Primosphere.Domain.Dynacore.Sphere;
using Nornir.Component.Primosphere.Motion.Dynacore.Elliptical;
using Nornir.Engine.Domain;
using Nornir.Engine.System;

namespace Nornir.System.Primosphere.Irradiance.Sphere;

public sealed record Params(
    double DistanceToParent,
    double OrbitalAngle,
    double AxialTilt,
    double RotationAngle,
    double StellarLuminosity
);

public sealed class Dynacore(
    IStoreReader storeReader,
    IStoreWriter storeWriter,
    IGeoGrid grid,
    IReadOnlyDictionary<CellId, int> cellIndex) : IDynacore<Params>
{

    private const double SolarLuminosity = 3.828e26;
    private const double LuminosityKnobMid = 50.0;
    private const double LuminosityLogRange = 13.0;

    public DynacoreResult Compute(DynacoreRequest<Params> request)
    {
        var distanceToParent = storeReader.GetScalar(domainId, MotionEllipticalFields.DistanceToParent);
        var orbitalAngle = storeReader.GetScalar(domainId, MotionEllipticalFields.OrbitalAngle);
        var axialTilt = storeReader.GetScalar(domainId, DomainSphereFields.AxialTilt);
        var rotationAngle = storeReader.GetScalar(domainId, DomainSphereFields.RotationAngle);

        if (AnyFieldIsMissing(distanceToParent, orbitalAngle, axialTilt, rotationAngle))
        {
            return new DynacoreResult(DynacoreStatus.Deferred, new DynacoreMetrics());
        }

        var luminosityWatts = KnobToLuminosity(config.Luminosity);
        var fluxAtDistance = FluxAtDistance(luminosityWatts, distanceToParent);
        var starDirection = StarDirection(orbitalAngle);
        var writeBuffer = storeWriter.GetWriteBuffer(domainId, IrradianceSphereFields.Irradiance);

        foreach (var cellId in cells.Cells)
        {
            var center = grid.CenterOf(cellId);
            var normal = CellNormal(center, rotationAngle, axialTilt);
            writeBuffer[cellIndex[cellId]] = LambertFlux(fluxAtDistance, normal, starDirection);
        }

        return new DynacoreResult(DynacoreStatus.Ok, new DynacoreMetrics());
    }

    private static bool AnyFieldIsMissing(double distanceToParent, double orbitalAngle, double axialTilt,
        double rotationAngle) =>
        double.IsNaN(distanceToParent) || double.IsNaN(orbitalAngle) ||
        double.IsNaN(axialTilt) || double.IsNaN(rotationAngle);

    private static double LambertFlux(double fluxAtDistance, Coords3D normal, Coords3D starDirection)
    {
        var cosineFactor = Lambert.CosineFactor(normal, starDirection);
        return cosineFactor > 0.0 ? fluxAtDistance * cosineFactor : 0.0;
    }

    private static double KnobToLuminosity(double knob)
    {
        var exponent = (knob - LuminosityKnobMid) / LuminosityKnobMid * LuminosityLogRange;
        return SolarLuminosity * Math.Pow(10.0, exponent);
    }

    private static double FluxAtDistance(double luminosity, double distance) =>
        luminosity / (4.0 * Math.PI * distance * distance);

    private static Coords3D StarDirection(double orbitalAngle) =>
        new(Math.Cos(orbitalAngle), Math.Sin(orbitalAngle), 0.0);

    private static Coords3D CellNormal(LatLng center, double rotationAngle, double axialTilt)
    {
        var lat = center.Lat * Math.PI / 180.0;
        var lng = (center.Lng * Math.PI / 180.0) + rotationAngle;

        var x = Math.Cos(lat) * Math.Cos(lng);
        var y = Math.Cos(lat) * Math.Sin(lng);
        var z = Math.Sin(lat);

        var tiltedX = (x * Math.Cos(axialTilt)) + (z * Math.Sin(axialTilt));
        var tiltedZ = (-x * Math.Sin(axialTilt)) + (z * Math.Cos(axialTilt));

        return new Coords3D(tiltedX, y, tiltedZ);
    }

}
