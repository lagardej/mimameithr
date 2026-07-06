using Friflo.Engine.ECS;
using UnitsNet;

namespace Kjarni.Nornir.Wyrd.Eldr;

/// <summary>Radiative output of a star.</summary>
[ComponentKey("geimr-luminosity")]
public struct LuminosityC : IComponent
{
    /// <summary>Total power radiated across all wavelengths.</summary>
    public Luminosity Luminosity;
}
