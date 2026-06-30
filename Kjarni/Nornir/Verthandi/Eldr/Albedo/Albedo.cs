using Friflo.Engine.ECS;
using Kjarni.Brunnr.Autodoc;
using UnitsNet;

namespace Kjarni.Nornir.Verthandi.Eldr.Albedo;

/// <summary>Surface reflectivity of a cell.</summary>
[ComponentKey("albedo")]
[Component("Eldr/Albedo")]
public struct AlbedoC : IComponent
{
    /// <summary>
    ///     Fraction of incoming solar radiation reflected by this cell's surface. Ranges from 0 (full absorption) to 1
    ///     (full reflection).
    /// </summary>
    [State("-", "Fraction of incoming solar radiation reflected by this cell's surface.")]
    public Ratio Value;
}
