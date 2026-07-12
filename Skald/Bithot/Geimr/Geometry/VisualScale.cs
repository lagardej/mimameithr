using UnitsNet;

namespace Bithot.Geimr.Geometry;

/// <summary>
///     Maps real body radius to a bounded scene-space radius. Real body radius spans 1 km (asteroid) to
///     1e9 km (supergiant star) — 9 orders of magnitude, far beyond what float32 scene coordinates can
///     hold without precision collapse. Log-scale into a fixed, bounded scene-space range instead:
///     preserves relative size ordering between bodies (star &gt; planet &gt; moon) without letting real
///     units leak into scene-space distances.
/// </summary>
public static class VisualScale
{
    private const double MinRealRadiusKm = 1.0;
    private const double MaxRealRadiusKm = 1_000_000_000.0;
    private const float MinVisualRadius = 0.05f;
    private const float MaxVisualRadius = 40f;

    /// <summary>Maps a physical <paramref name="radius" /> to a bounded scene-space radius via log scale.</summary>
    public static float ToVisualRadius(Length radius)
    {
        var km = Math.Clamp(radius.Kilometers, MinRealRadiusKm, MaxRealRadiusKm);
        var t = (Math.Log10(km) - Math.Log10(MinRealRadiusKm)) /
                (Math.Log10(MaxRealRadiusKm) - Math.Log10(MinRealRadiusKm));
        return (float)(MinVisualRadius + t * (MaxVisualRadius - MinVisualRadius));
    }
}