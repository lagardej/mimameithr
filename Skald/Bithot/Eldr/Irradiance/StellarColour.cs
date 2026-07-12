using UnitsNet;

namespace Bithot.Eldr.Irradiance;

/// <summary>Rendering-only colour approximation for a star. Not a simulation concern — Nornir has no notion of RGB.</summary>
public static class StellarColour
{
    /// <summary>Lower bound of the fit's valid temperature range.</summary>
    private const double MinValidTemperatureKelvin = 1000.0;

    /// <summary>Upper bound of the fit's valid temperature range.</summary>
    private const double MaxValidTemperatureKelvin = 40000.0;

    /// <summary>The fit's independent variable is temperature divided by this value ("Kelvin100").</summary>
    private const double Kelvin100Divisor = 100.0;

    /// <summary>Channel values are fitted on a 0–255 scale; divide by this to normalize to [0, 1].</summary>
    private const double ByteScale = 255.0;

    /// <summary>Kelvin100 threshold above which red is saturated and green/blue switch to their high-temperature fits.</summary>
    private const double RedGreenBreakpointKelvin100 = 66.0;

    /// <summary>Kelvin100 threshold below which blue is fully absorbed (zero).</summary>
    private const double BlueZeroThresholdKelvin100 = 19.0;

    // Red, Kelvin100 > breakpoint: Red = RedCoefficient * (Kelvin100 - RedExponentOffset) ^ RedExponent
    private const double RedCoefficient = 329.698727446;
    private const double RedExponentOffset = 60.0;
    private const double RedExponent = -0.1332047592;

    // Green, Kelvin100 <= breakpoint: Green = GreenLowSlope * ln(Kelvin100) - GreenLowIntercept
    private const double GreenLowSlope = 99.4708025861;
    private const double GreenLowIntercept = 161.1195681661;

    // Green, Kelvin100 > breakpoint: Green = GreenHighCoefficient * (Kelvin100 - GreenHighExponentOffset) ^ GreenHighExponent
    private const double GreenHighCoefficient = 288.1221695283;
    private const double GreenHighExponentOffset = 60.0;
    private const double GreenHighExponent = -0.0755148492;

    // Blue, between the two thresholds: Blue = BlueSlope * ln(Kelvin100 - BlueLogOffset) - BlueIntercept
    private const double BlueSlope = 138.5177312231;
    private const double BlueLogOffset = 10.0;
    private const double BlueIntercept = 305.0447927307;

    /// <summary>
    ///     Approximate blackbody colour of a star at a given effective temperature, normalized to [0, 1] per channel.
    ///     Piecewise polynomial fit valid for temperatures between 1000 K and 40000 K; clamped outside that range.
    /// </summary>
    /// <param name="temperature">Effective temperature of the star.</param>
    /// <remarks>
    ///     Tanner Helland's blackbody colour approximation:
    ///     <see href="https://tannerhelland.com/2012/09/18/convert-temperature-rgb-algorithm-code.html">
    ///         How to Convert Temperature (K) to RGB: Algorithm and Sample Code
    ///     </see>
    ///     .
    ///     Fitted by regression against Mitchell Charity's blackbody spectrum tables.
    ///     Valid for 1000 K–40000 K; the fit is expressed as a function of temperature/100 (Kelvin100)
    ///     and channel values are on the source's native 0–255 scale before normalization.
    /// </remarks>
    public static (double R, double G, double B) BlackbodyColour(Temperature temperature)
    {
        var kelvin100 = Math.Clamp(temperature.Kelvins, MinValidTemperatureKelvin, MaxValidTemperatureKelvin) /
                        Kelvin100Divisor;

        var r = kelvin100 <= RedGreenBreakpointKelvin100
            ? ByteScale
            : RedCoefficient * Math.Pow(kelvin100 - RedExponentOffset, RedExponent);

        var g = kelvin100 <= RedGreenBreakpointKelvin100
            ? GreenLowSlope * Math.Log(kelvin100) - GreenLowIntercept
            : GreenHighCoefficient * Math.Pow(kelvin100 - GreenHighExponentOffset, GreenHighExponent);

        var b = kelvin100 >= RedGreenBreakpointKelvin100
            ? ByteScale
            : kelvin100 <= BlueZeroThresholdKelvin100
                ? 0.0
                : BlueSlope * Math.Log(kelvin100 - BlueLogOffset) - BlueIntercept;

        return (
            Math.Clamp(r / ByteScale, 0.0, 1.0),
            Math.Clamp(g / ByteScale, 0.0, 1.0),
            Math.Clamp(b / ByteScale, 0.0, 1.0));
    }
}