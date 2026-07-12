using UnitsNet;

namespace Nornir.Hlothyn.Mantle;

/// <summary>Provides physics calculations for planetary mantle properties.</summary>
public static class MantlePhysics
{
    /// <summary>
    ///     Derives asthenospheric heat flux from body mass, radius, and age.
    ///     Uses specific radiogenic heat production scaled by mass, decayed by age,
    ///     and normalized by surface area. No Earth reference values.
    /// </summary>
    public static HeatFlux AsthenosphericHeatFlux(Duration age, Mass mass, Length radius)
    {
        // Specific radiogenic heat production: mean bulk-silicate value (W/kg).
        const double alpha = 9.6e-12;
        // Effective radiogenic decay constant (s⁻¹), corresponds to ~2.5 Gyr half-life.
        const double lambda = 8.8e-18;

        var surfaceArea = 4.0 * Math.PI * radius.Meters * radius.Meters;
        var decay = Math.Exp(-lambda * age.Seconds);

        return HeatFlux.FromWattsPerSquareMeter(alpha * mass.Kilograms * decay / surfaceArea);
    }
}
