using static Kjarni.Nornir.Eldr.Luminosity.SetLuminosityScale;
using static Kjarni.Nornir.Geimr.Geometry.SetGeometryScale;
using static Kjarni.Nornir.Geimr.Physics.SetPhysicsScale;
using static Kjarni.Nornir.Geimr.Rotation.SetRotationScale;

namespace Skald.Bithot;

/// <summary>
///     Convenience helpers to convert real-world quantities to scale values used in generation-phase commands.
///     These invert the PiecewiseExponentialScale transformations defined in the command scales.
/// </summary>
public static class BootstrapScales
{
    /// <summary>Converts a radius in kilometres to its scale value (1–1000).</summary>
    public static uint RadiusKm(double km) => RadiusScale.Inverse(km);

    /// <summary>Converts an age in Julian years to its scale value (1–1000).</summary>
    public static uint AgeYears(double years) => AgeScale.Inverse(years);

    /// <summary>Converts a mass in kilograms to its scale value (1–1000).</summary>
    public static uint MassKg(double kg) => MassScale.Inverse(kg);

    /// <summary>Converts a rotation period in seconds to its scale value (1–1000).</summary>
    public static uint RotationPeriodSeconds(double seconds) => RotationPeriodScale.Inverse(seconds);

    /// <summary>Converts a rotation period in hours to its scale value (1–1000).</summary>
    public static uint RotationPeriodHours(double hours) => RotationPeriodSeconds(hours * 3600);

    /// <summary>Converts a rotation period in days to its scale value (1–1000).</summary>
    public static uint RotationPeriodDays(double days) => RotationPeriodSeconds(days * 86400);

    /// <summary>Converts a luminosity in solar luminosities (L☉) to its scale value (1–1000).</summary>
    public static uint LuminositySolar(double solarLuminosities) => LuminosityScale.Inverse(solarLuminosities);
}
