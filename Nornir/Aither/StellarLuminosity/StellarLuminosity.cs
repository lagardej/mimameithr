using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Aither.StellarLuminosity;

/// <summary>Radiative output of a star.</summary>
[ComponentKey("stellar-luminosity")]
[Component(group: "Aither/StellarLuminosity")]
public struct StellarLuminosityC : IComponent
{
    /// <summary>Total power radiated across all wavelengths.</summary>
    [Setting("W", "Total power radiated across all wavelengths.")]
    public Luminosity Luminosity;
}
