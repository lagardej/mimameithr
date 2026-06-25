using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Aethersphere.StellarLuminosity;

[ComponentKey("stellar-luminosity")]
public struct StellarLuminosityC : IComponent
{
    /// <summary>Total power radiated by the star across all wavelengths. Primary energy input for the entire simulation.</summary>
    public Luminosity Luminosity;
}
