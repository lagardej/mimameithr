using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Lithosphere;

/// <summary>Provides physics calculations for planetary crustal properties.</summary>
public static class CrustalPhysics
{
    /// <summary>
    ///     Derives crustal thickness from composition, gravity, and asthenospheric heat flux.
    ///     Based on isostatic equilibrium: thickness = heatFlux / (density × gravity × coolingRate).
    ///     Composition determines density and radiogenic cooling rate.
    /// </summary>
    public static Length CrustThickness(CrustComposition composition, Acceleration gravity, HeatFlux heatFlux)
    {
        // Crustal density by composition (kg/m³).
        const double felsicDensity = 2700.0;
        const double maficDensity = 3000.0;

        // Radiogenic cooling rate by composition (W/m³): heat generated per unit volume.
        // Felsic crust is more enriched in radiogenic elements than mafic.
        const double felsicCoolingRate = 2.5e-7;
        const double maficCoolingRate = 5.0e-8;

        var density = composition == CrustComposition.Felsic ? felsicDensity : maficDensity;
        var coolingRate = composition == CrustComposition.Felsic ? felsicCoolingRate : maficCoolingRate;

        var meters = heatFlux.WattsPerSquareMeter / (density * gravity.MetersPerSecondSquared * coolingRate);
        return Length.FromMeters(meters);
    }
}
