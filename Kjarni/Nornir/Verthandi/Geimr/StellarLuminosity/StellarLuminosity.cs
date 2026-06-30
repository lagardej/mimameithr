using Friflo.Engine.ECS;
using Kjarni.Brunnr.Autodoc;
using UnitsNet;

namespace Kjarni.Nornir.Verthandi.Geimr.StellarLuminosity;

/// <summary>Radiative output of a star.</summary>
[ComponentKey("stellar-luminosity")]
[Component("Geimr/StellarLuminosity")]
public struct StellarLuminosityC : IComponent
{
    /// <summary>Total power radiated across all wavelengths.</summary>
    [Setting("W", "Total power radiated across all wavelengths.")]
    public Luminosity Luminosity;
}
