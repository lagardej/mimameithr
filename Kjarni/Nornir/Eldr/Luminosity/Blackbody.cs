using UnitsNet;

namespace Nornir.Eldr.Luminosity;

/// <summary>Static utility methods for blackbody radiation calculations.</summary>
public static class Blackbody
{
    private const double StefanBoltzmann = 5.670374419e-8;

    /// <summary>Effective temperature of a star, derived from luminosity and radius via the Stefan–Boltzmann law.</summary>
    /// <param name="luminosity">Total power radiated by the star across all wavelengths.</param>
    /// <param name="radius">Physical radius of the star.</param>
    public static Temperature EffectiveTemperature(UnitsNet.Luminosity luminosity, Length radius)
    {
        var surfaceArea = 4.0 * Math.PI * radius.Meters * radius.Meters;
        var kelvin = Math.Pow(luminosity.Watts / (surfaceArea * StefanBoltzmann), 0.25);
        return Temperature.FromKelvins(kelvin);
    }
}
