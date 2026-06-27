using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Element.Pyr.Albedo;

/// <summary>Surface reflectivity of a cell.</summary>
[ComponentKey("albedo")]
public struct AlbedoC : IComponent
{
    /// <summary>Fraction of incoming solar radiation reflected by this cell's surface. Ranges from 0 (full absorption) to 1 (full reflection).</summary>
    public Ratio Value;
}
