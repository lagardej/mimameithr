using Nornir.Engine.Domain;

namespace Nornir.Component.Primosphere.Motion.Dynacore.Elliptical;

/// <summary>Store fields produced by the elliptical motion dynacore. All values in SI units.</summary>
public static class MotionEllipticalFields
{
    /// <summary>Distance from the domain to its parent body, in metres.</summary>
    public static readonly FieldId DistanceToParent = new(MotionEllipticalDynacore.Key, 0);

    /// <summary>Current orbital angle relative to the reference direction, in radians.</summary>
    public static readonly FieldId OrbitalAngle = new(MotionEllipticalDynacore.Key, 1);
}
