using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Pyr.Albedo;

/// <summary>Surface reflectivity of a cell.</summary>
[ComponentKey("albedo")]
[Component(group: "Pyr/Albedo")]
public struct AlbedoC : IComponent
{
    /// <summary>
    ///     Fraction of incoming solar radiation reflected by this cell's surface. Ranges from 0 (full absorption) to 1
    ///     (full reflection).
    /// </summary>
    [State("-", "Fraction of incoming solar radiation reflected by this cell's surface.")]
    public Ratio Value;
}
