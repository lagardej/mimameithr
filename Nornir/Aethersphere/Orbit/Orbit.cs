using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Aethersphere.Orbit;

[ComponentKey("orbit")]
public struct OrbitC : IComponent
{
    /// <summary>Semi-major axis of the orbit. Used to compute distance from star.</summary>
    public Length SemiMajorAxis;

    /// <summary>Shape of the orbital ellipse. 0 = circular, approaching 1 = parabolic limit.</summary>
    public double Eccentricity;

    /// <summary>Sidereal orbital period. Used to derive mean motion.</summary>
    public Duration OrbitalPeriod;

    /// <summary>Current mean anomaly. Integrated uniformly from mean motion each tick.</summary>
    public Angle MeanAnomaly;

    /// <summary>Current true anomaly. Derived from mean anomaly via Kepler's equation each tick.</summary>
    public Angle OrbitalAngle;

    /// <summary>
    ///     Current distance between the body and its parent star. Derived each tick from semi-major axis, eccentricity,
    ///     and true anomaly.
    /// </summary>
    public Length DistanceFromStar;
}
