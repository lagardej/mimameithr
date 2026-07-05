using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Nornir.Wyrd.Geimr;

/// <summary>Keplerian orbital state of a planetary body around its parent star.</summary>
[ComponentKey("geimr-orbit")]
public struct OrbitC : IComponent
{
    /// <summary>Current distance between the body and its parent star.</summary>
    /// <remarks>Computed from <see cref="SemiMajorAxis" />, <see cref="Eccentricity" />, and <see cref="OrbitalAngle" />.</remarks>
    public Length DistanceFromStar;

    /// <summary>Shape of the orbital ellipse. 0 = circular, approaching 1 = parabolic limit.</summary>
    public double Eccentricity;

    /// <summary>Current mean anomaly.</summary>
    public Angle MeanAnomaly;

    /// <summary>Current true anomaly.</summary>
    /// <remarks>Computed from <see cref="MeanAnomaly" /> via Kepler's equation.</remarks>
    public Angle OrbitalAngle;

    /// <summary>Sidereal orbital period.</summary>
    public Duration OrbitalPeriod;

    /// <summary>Semi-major axis of the orbit.</summary>
    public Length SemiMajorAxis;
}

/// <summary>Links an orbited entity to its parent entity.</summary>
[ComponentKey("geimr-orbit-parent-ref")]
public struct OrbitParentC : ILinkComponent
{
    /// <summary>The entity this body orbits around.</summary>
    public Entity Parent;

    /// <inheritdoc />
    public Entity GetIndexedValue() => Parent;
}
