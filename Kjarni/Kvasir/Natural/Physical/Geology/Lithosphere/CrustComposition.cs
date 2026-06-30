namespace Kjarni.Kvasir.Natural.Physical.Geology.Lithosphere;

/// <summary>Crust composition classification.</summary>
public enum CrustComposition
{
    /// <summary>Silicon-rich crust. Typical of continental crust. Lower density, higher elevation.</summary>
    Felsic,

    /// <summary>Magnesium- and iron-rich crust. Typical of oceanic crust. Higher density, lower elevation.</summary>
    Mafic
}

/// <summary>Extension methods for <see cref="CrustComposition" />.</summary>
public static class CrustCompositionExtensions
{
    /// <summary>Bulk density of the crust composition in kg/m³.</summary>
    public static double Density(this CrustComposition composition) => composition switch
    {
        CrustComposition.Felsic => 2700.0,
        CrustComposition.Mafic => 3000.0,
        _ => throw new ArgumentOutOfRangeException(nameof(composition))
    };
}
