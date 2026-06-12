using Nornir.Config.Primosphere.Orbit;
using Nornir.Engine.Domain;
using Nornir.Engine.System;

namespace Nornir.Component.Primosphere.Motion.Dynacore.Elliptical;

/// <summary>
///     Advances orbital state for a body on an elliptical orbit.
/// </summary>
public sealed class MotionEllipticalDynacore(IStoreWriter storeWriter) : IDynacore<EllipticalConfig>
{
    public const string Key = "primosphere-motion-elliptical";

    /// <inheritdoc />
    public DynacoreId Id() => new(Key);

    /// <inheritdoc />
    public DynacoreResult Compute(EllipticalConfig config, string domainId, CellSet cells, double elapsedSeconds)
    {
        var orbitalAngle = (elapsedSeconds / config.OrbitalPeriod * 2.0 * Math.PI)
                           + (config.InitialTrueAnomaly * Math.PI / 180.0);

        var distanceToParent = config.SemiMajorAxis * 1000.0
                                                    * (1.0 - (config.Eccentricity * config.Eccentricity))
                               / (1.0 + (config.Eccentricity * Math.Cos(orbitalAngle)));

        storeWriter.SetScalar(domainId, MotionEllipticalFields.OrbitalAngle, orbitalAngle);
        storeWriter.SetScalar(domainId, MotionEllipticalFields.DistanceToParent, distanceToParent);

        return new DynacoreResult(DynacoreStatus.Ok, new DynacoreMetrics());
    }
}
