using Kvasir.Science.Maths.Geometry.Spherics;
using Nornir.Config.Primosphere.Domain;
using Nornir.Engine.Domain;
using Nornir.Engine.System;
using UnitsNet;

namespace Nornir.Component.Primosphere.Domain.Dynacore.Sphere;

/// <summary>
///     Advances domain-level physical state for a spherical body.
/// </summary>
public sealed class DomainSphereDynacore(IStoreWriter storeWriter) : IDynacore<SphereConfig>
{
    public const string Key = "primosphere-domain-sphere";

    /// <inheritdoc />
    public DynacoreId Id() => new(Key);

    /// <inheritdoc />
    public DynacoreResult Compute(SphereConfig config, string domainId, CellSet cells, double elapsedSeconds)
    {
        var axialTilt = config.AxialTilt * Math.PI / 180.0;
        var initialRotationAngle = config.InitialRotationAngle * Math.PI / 180.0;
        var radius = config.Radius * 1000.0;

        var rotationAngle = SphericalGeometry.RotationAngle(
            Duration.FromSeconds(config.RotationPeriod),
            (ulong) elapsedSeconds) + initialRotationAngle;

        storeWriter.SetScalar(domainId, DomainSphereFields.Radius, radius);
        storeWriter.SetScalar(domainId, DomainSphereFields.AxialTilt, axialTilt);
        storeWriter.SetScalar(domainId, DomainSphereFields.RotationAngle, rotationAngle);
        storeWriter.SetScalar(domainId, DomainSphereFields.SurfaceGravity, config.SurfaceGravity);

        return new DynacoreResult(DynacoreStatus.Ok, new DynacoreMetrics());
    }
}
