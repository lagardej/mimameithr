using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Pyrosphere.Irradiance;

[ComponentKey("irradiance")]
public struct IrradianceC : IComponent
{
    /// <summary>
    ///     Incoming solar flux at this cell's surface. Derived from stellar luminosity, orbital distance, and solar
    ///     zenith angle.
    /// </summary>
    public UnitsNet.Irradiance Insolation;

    /// <summary>Angle between the sun and the local vertical at this cell. Zero at solar noon directly below the sun.</summary>
    public Angle SolarZenithAngle;

    /// <summary>Whether this cell is currently on the sunlit side of the body.</summary>
    public bool IsDaytime;
}
