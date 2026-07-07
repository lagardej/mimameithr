using Friflo.Engine.ECS;
using Kjarni.Brunnr.Engine.Cell;
using Kjarni.Nornir.Hlothyn.Tectonics.MobileLid;
using Kjarni.Nornir.Hlothyn.Tectonics.StagnantLid;

namespace Kjarni.Nornir.Hlothyn.Tectonics;

internal static class RegimeInvariant
{
    internal static void RemoveAllRegimeComponents(EntityStore store, Entity body)
    {
        body.RemoveComponent<TectonicsMobileLidC>();
        body.RemoveComponent<TectonicsStagnantLidC>();

        store.Query<CellParentRefC>().ForEachEntity((ref parentRef, cellEntity) =>
        {
            if (parentRef.Parent != body)
            {
                return;
            }

            cellEntity.RemoveComponent<TectonicsMobileLidC>();
            cellEntity.RemoveComponent<TectonicsStagnantLidC>();
        });
    }
}
