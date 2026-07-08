using Kjarni.Kvasir.Foundation.Grid;
using UnitsNet;

namespace Kjarni.Nornir.Hlothyn.Orogeny;

/// <summary>All inputs to the orogeny simulation.</summary>
public sealed record Parameters
{
    /// <summary>Age of the body. Upper bound for orogenic onset sampling.</summary>
    public required Duration BodyAge { get; init; }

    /// <summary>
    ///     Exponent applied to the onset sample. Below 1 skews onset toward <see cref="BodyAge" /> (young belts).
    ///     Above 1 skews onset toward zero (old belts).
    /// </summary>
    public required double AgeBiasExponent { get; init; }

    /// <summary>Duration over which relief potential decays by half.</summary>
    public required Duration ReliefHalfLife { get; init; }

    /// <summary>Grid to operate on.</summary>
    public required IGrid Grid { get; init; }
}
