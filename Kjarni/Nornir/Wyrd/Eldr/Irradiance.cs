using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Nornir.Wyrd.Eldr;

/// <summary>Stellar irradiance state at a surface cell.</summary>
[ComponentKey("eldr-irradiance")]
public struct IrradianceC : IComponent
{
    /// <summary>Incoming stellar flux at this cell's surface.</summary>
    /// <remarks>Computed from stellar luminosity, orbital distance, and stellar zenith angle.</remarks>
    public Irradiance Insolation;

    /// <summary>Whether this cell is currently on the lit side of the body.</summary>
    public bool IsDaytime;

    /// <summary>Angle between the star and the local vertical at this cell. Zero at local culmination directly below the star.</summary>
    public Angle ZenithAngle;
}
