using Friflo.Engine.ECS;
using Godot;

namespace Skald.Bithot.Geimr.Geometry;

/// <summary>
///     Scene-space radius for a body, derived from <see cref="Kjarni.Nornir.Geimr.Geometry.GeometryC.Radius" />
///     via <see cref="VisualScale" />. Render-only — never read or written by <c>Kjarni.Nornir</c>.
/// </summary>
[ComponentKey("bithot-visual-radius")]
public struct VisualRadiusC : IComponent
{
    /// <summary>Bounded scene-space radius. See <see cref="VisualScale" /> for the mapping.</summary>
    public float Value;
}

/// <summary>
///     Reference to a body's scene-space render node, set once when <see cref="BodyRenderer" /> creates it.
///     Lets other Bithot systems (position, rotation) find the node to push simulated transform state onto
///     without keeping their own lookup table.
/// </summary>
[ComponentKey("bithot-body-node")]
public struct BodyNodeC : IComponent
{
    /// <summary>The body's root scene node.</summary>
    public Node3D Node;
}