using Brunnr.Cell;
using Friflo.Engine.ECS;
using Nornir.Hlothyn.Tectonics.MobileLid;
using Nornir.Hlothyn.Tectonics.StagnantLid;

namespace Nornir.Hlothyn.Tectonics;

internal static class RegimeInvariant
{
    internal static void RemoveAllRegimeComponents(EntityStore store, Entity body)
    {
        body.RemoveComponent<TectonicsStagnantLidC>();

        store.Query<CellParentRefC>().ForEachEntity((ref parentRef, cellEntity) =>
        {
            if (parentRef.Parent != body)
            {
                return;
            }

            cellEntity.RemoveComponent<TectonicsBoundaryC>();
            cellEntity.RemoveComponent<TectonicsPlateC>();
            cellEntity.RemoveComponent<TectonicsStagnantLidC>();
        });
    }
}
