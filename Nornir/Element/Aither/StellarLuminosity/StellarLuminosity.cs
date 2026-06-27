using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Element.Aither.StellarLuminosity;

/// <summary>Radiative output of a star.</summary>
[ComponentKey("stellar-luminosity")]
public struct StellarLuminosityC : IComponent
{
    /// <summary>Total power radiated across all wavelengths.</summary>
    public Luminosity Luminosity;
}
