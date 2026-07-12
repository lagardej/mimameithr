using Friflo.Engine.ECS;
using System.Numerics;
using UnitsNet;

namespace Nornir.Geimr.Position;

/// <summary>Cartesian position and velocity of a planetary body.</summary>
[ComponentKey("geimr-position")]
public struct PositionC : IComponent
{
    /// <summary>Coordinates relative to the system origin.</summary>
    public Length X;

    /// <summary>Coordinates relative to the system origin.</summary>
    public Length Y;

    /// <summary>Coordinates relative to the system origin.</summary>
    public Length Z;

    /// <summary>Velocity component along X.</summary>
    public Speed VelocityX;

    /// <summary>Velocity component along Y.</summary>
    public Speed VelocityY;

    /// <summary>Velocity component along Z.</summary>
    public Speed VelocityZ;
}

/// <summary>Links an orbiting entity to the entity it orbits.</summary>
[ComponentKey("geimr-orbit-parent-ref")]
public struct OrbitParentC : ILinkComponent
{
    /// <summary>The entity this body orbits around.</summary>
    public Entity Parent;

    /// <inheritdoc />
    public Entity GetIndexedValue() => Parent;
}

/// <summary>
///     Keplerian orbital state of a planetary body around its parent, derived once from the initial state
///     vector given to <see cref="SetPosition" /> and propagated analytically each tick into <see cref="PositionC" />.
/// </summary>
/// <remarks>
///     Lives beside <see cref="PositionC" /> rather than in its own domain folder: the two never change
///     independently — any change to how an orbit is derived or propagated touches both. See
///     <c>.kanban/2_Doing/orbit-system-rewrite.md</c>.
/// </remarks>
[ComponentKey("geimr-orbit")]
public struct OrbitC : IComponent
{
    /// <summary>Current distance between the body and its parent.</summary>
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

    /// <summary>
    ///     Unit vector from the parent toward periapsis, in the orbital plane. Defines the reference
    ///     direction from which <see cref="OrbitalAngle" /> is measured.
    /// </summary>
    public Vector3 PeriapsisDirection;

    /// <summary>
    ///     Unit vector normal to the orbital plane, right-handed with <see cref="OrbitalAngle" /> increasing
    ///     from <see cref="PeriapsisDirection" />.
    /// </summary>
    public Vector3 OrbitNormal;
}
