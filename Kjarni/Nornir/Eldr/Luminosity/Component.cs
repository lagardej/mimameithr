using Friflo.Engine.ECS;

namespace Nornir.Eldr.Luminosity;

/// <summary>Radiative output of a star.</summary>
[ComponentKey("eldr-luminosity")]
public struct LuminosityC : IComponent
{
    /// <summary>Total power radiated across all wavelengths.</summary>
    public UnitsNet.Luminosity Luminosity;
}
