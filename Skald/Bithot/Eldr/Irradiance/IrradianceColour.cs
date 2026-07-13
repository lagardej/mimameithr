using Godot;
using UnitsNet;

namespace Bithot.Eldr.Irradiance;

/// <summary>Rendering-only colour ramp for insolation. Not a simulation concern.</summary>
public static class IrradianceColour
{
    /// <summary>Upper display bound, roughly the solar constant at 1 AU. Values above this clamp to <see cref="DayColor" />.</summary>
    private const double MaxWattsPerSquareMeter = 1400.0;

    private static readonly Color NightColor = new(0.02f, 0.02f, 0.08f);
    private static readonly Color DayColor = new(1f, 0.85f, 0.4f);

    /// <summary>Maps insolation linearly onto a night→day colour ramp, clamped to [0, 1].</summary>
    public static Color FromInsolation(UnitsNet.Irradiance insolation)
    {
        var t = (float) Math.Clamp(insolation.WattsPerSquareMeter / MaxWattsPerSquareMeter, 0.0, 1.0);
        return NightColor.Lerp(DayColor, t);
    }
}
