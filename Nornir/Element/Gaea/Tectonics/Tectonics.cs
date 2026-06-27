using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Geology.Lithosphere;
using Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;
using UnitsNet;

namespace Nornir.Element.Gaea.Tectonics;

/// <summary>Tectonic state of a surface cell.</summary>
[ComponentKey("tectonics")]
public struct TectonicsC : IComponent
{
    /// <summary>Dominant crust composition at this cell.</summary>
    public CrustComposition CrustComposition;

    /// <summary>Thickness of the crust at this cell.</summary>
    public Length CrustalThickness;

    /// <summary>Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.</summary>
    public Speed VerticalDisplacementRate;

    /// <summary>Tectonic boundary type at this cell.</summary>
    public BoundaryType BoundaryType;
}

/// <summary>
///     Computes <see cref="TectonicsC" /> for each cell entity.
///     One-shot at world gen; only re-runs on explicit forcing.
/// </summary>
public sealed class TectonicsSystem : QuerySystem<TectonicsC>
{
    protected override void OnUpdate() =>
        Query.ForEachEntity((ref tectonics, _) => tectonics = ComputeTectonics(tectonics));

    private static TectonicsC ComputeTectonics(TectonicsC tectonics) =>
        // TODO: derive from world gen parameters.
        tectonics;
}
