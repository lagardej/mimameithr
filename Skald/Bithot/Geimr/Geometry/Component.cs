using Friflo.Engine.ECS;

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
