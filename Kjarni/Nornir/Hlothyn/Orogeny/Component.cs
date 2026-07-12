using Friflo.Engine.ECS;
using UnitsNet;

namespace Nornir.Hlothyn.Orogeny;

/// <summary>Orogenic state of a surface cell.</summary>
[ComponentKey("hlothyn-orogeny")]
public struct OrogenyC : IComponent
{
    /// <summary>Time elapsed since this cell's orogenic belt began forming.</summary>
    public Duration OrogenicAge;

    /// <summary>Crustal shortening accumulated at this cell since orogenic onset.</summary>
    public Length AccumulatedCrustalShortening;

    /// <summary>Fraction of original relief remaining at this cell. 1 = freshly built, 0 = eroded flat.</summary>
    /// <remarks>Computed from <see cref="OrogenicAge" /> via exponential decay.</remarks>
    public Ratio ReliefPotential;
}
