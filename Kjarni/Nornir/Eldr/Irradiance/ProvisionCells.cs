using Brunnr.Cell;
using Friflo.Engine.ECS.Systems;
using Nornir.Geimr.Geometry;

namespace Nornir.Eldr.Irradiance;

/// <summary>
///     Adds a default <see cref="IrradianceC" /> to any cell that doesn't have one yet. Cell creation
///     (<see cref="AddGrid" />) is domain-agnostic and doesn't know about Eldr's
///     per-cell data, so provisioning happens here instead. Purely structural — actual insolation values
///     are written every tick by <see cref="UpdateIrradiance" />, which requires the component to already
///     be present to match its query.
/// </summary>
/// <remarks>
///     Adds via the system's <c>CommandBuffer</c>, not directly — <c>Query.ForEachEntity</c> throws
///     <c>StructuralChangeException</c> on direct structural changes while iterating.
/// </remarks>
public sealed class ProvisionIrradiance : QuerySystem<CellIdentityC, CellParentRefC>
{
    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var buffer = CommandBuffer;

        Query.ForEachEntity((ref identity, ref parentRef, entity) =>
        {
            _ = identity;
            _ = parentRef;

            if (!entity.HasComponent<IrradianceC>())
            {
                buffer.AddComponent(entity.Id, new IrradianceC());
            }
        });
    }
}
