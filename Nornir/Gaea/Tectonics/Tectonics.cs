using Brunnr.Autodoc;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Kvasir.Natural.Physical.Geology.Lithosphere;
using Kvasir.Natural.Physical.Geology.Lithosphere.Tectonics;
using UnitsNet;

namespace Nornir.Gaea.Tectonics;

/// <summary>Tectonic state of a surface cell.</summary>
[ComponentKey("tectonics")]
[Component(summary: "Tectonic state of a surface cell.", group: "Gaea/Tectonics")]
public struct TectonicsC : IComponent
{
    /// <summary>Dominant crust composition at this cell.</summary>
    [State("-", "Dominant crust composition at this cell.")]
    public CrustComposition CrustComposition;

    /// <summary>Thickness of the crust at this cell.</summary>
    [State("m", "Thickness of the crust at this cell.")]
    public Length CrustalThickness;

    /// <summary>Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.</summary>
    [State("m/s", "Rate of vertical crustal displacement at this cell. Positive = uplift, negative = subsidence.")]
    public Speed VerticalDisplacementRate;

    /// <summary>Tectonic boundary type at this cell.</summary>
    [State("-", "Tectonic boundary type at this cell.")]
    public BoundaryType BoundaryType;
}

/// <summary>
///     Computes <see cref="TectonicsC" /> for each cell entity.
///     One-shot at world gen; only re-runs on explicit forcing.
/// </summary>
public sealed class TectonicsSystem : QuerySystem<TectonicsC>
{
    /// <inheritdoc />
    protected override void OnUpdate() =>
        Query.ForEachEntity((ref tectonics, _) => tectonics = ComputeTectonics(tectonics));

    private static TectonicsC ComputeTectonics(TectonicsC tectonics) =>
        // TODO: derive from world gen parameters.
        tectonics;
}
