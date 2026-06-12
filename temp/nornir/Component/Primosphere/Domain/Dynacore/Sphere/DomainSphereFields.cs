using Nornir.Engine.Domain;

namespace Nornir.Component.Primosphere.Domain.Dynacore.Sphere;

/// <summary>Store fields produced by the spherical domain dynacore. All values in SI units.</summary>
public static class DomainSphereFields
{
    /// <summary>Equatorial radius of the domain, in metres.</summary>
    public static readonly FieldId Radius = new(DomainSphereDynacore.Key, 0);

    /// <summary>Axial tilt of the domain relative to its orbital plane, in radians.</summary>
    public static readonly FieldId AxialTilt = new(DomainSphereDynacore.Key, 1);

    /// <summary>Current rotation angle of the domain around its axis, in radians.</summary>
    public static readonly FieldId RotationAngle = new(DomainSphereDynacore.Key, 2);

    /// <summary>Surface gravitational acceleration, in m/s².</summary>
    public static readonly FieldId SurfaceGravity = new(DomainSphereDynacore.Key, 3);
}
