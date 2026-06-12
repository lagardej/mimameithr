using Nornir.Config.Primosphere.Orbit;
using Nornir.Engine.Domain;
using Nornir.Engine.System;

namespace Nornir.Component.Primosphere.Motion.Dynacore.Fixed;

/// <summary>
///     Advances orbital state for a body at a fixed position.
/// </summary>
public sealed class MotionFixedDynacore(IStoreWriter storeWriter) : IDynacore<FixedConfig>
{
    public const string Key = "primosphere-motion-fixed";

    /// <inheritdoc />
    public DynacoreId Id() => new(Key);

    /// <inheritdoc />
    public DynacoreResult Compute(FixedConfig config, string domainId, CellSet cells, double elapsedSeconds)
    {
        storeWriter.SetScalar(domainId, MotionFixedFields.X, config.Position.X * 1000.0);
        storeWriter.SetScalar(domainId, MotionFixedFields.Y, config.Position.Y * 1000.0);
        storeWriter.SetScalar(domainId, MotionFixedFields.Z, config.Position.Z * 1000.0);

        return new DynacoreResult(DynacoreStatus.Ok, new DynacoreMetrics());
    }
}
