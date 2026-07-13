using Friflo.Engine.ECS;
using Godot;

namespace Bithot.Eldr.Irradiance;

/// <summary>
///     Render-only colour for a cell, derived from <see cref="Nornir.Eldr.Irradiance.IrradianceC.Insolation" />.
///     Never read or written by <c>Nornir</c>.
/// </summary>
[ComponentKey("bithot-irradiance-color")]
public struct IrradianceColorC : IComponent
{
    /// <summary>Colour representing this cell's current insolation.</summary>
    public Color Value;
}
