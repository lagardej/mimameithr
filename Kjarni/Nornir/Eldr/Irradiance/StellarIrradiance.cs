using UnitsNet;

namespace Nornir.Eldr.Irradiance;

/// <summary>Static utility methods for computing insolation at a planetary surface cell.</summary>
public static class StellarIrradiance
{
    /// <summary>Computes the stellar flux at a surface cell. Returns zero on the night side.</summary>
    /// <param name="luminosity">Total power radiated by the star across all wavelengths.</param>
    /// <param name="distanceFromStar">Distance between the body and its parent star.</param>
    /// <param name="zenithAngle">Stellar zenith angle at the cell.</param>
    /// <returns>Incoming stellar flux at the cell's surface.</returns>
    public static UnitsNet.Irradiance Insolation(UnitsNet.Luminosity luminosity, Length distanceFromStar,
        Angle zenithAngle)
    {
        var fluxWattsPerM2 = luminosity.Watts / (4.0 * Math.PI * distanceFromStar.Meters * distanceFromStar.Meters);
        var watts = zenithAngle.Degrees < 90.0 ? fluxWattsPerM2 * Math.Cos(zenithAngle.Radians) : 0.0;
        return UnitsNet.Irradiance.FromWattsPerSquareMeter(watts);
    }
}
