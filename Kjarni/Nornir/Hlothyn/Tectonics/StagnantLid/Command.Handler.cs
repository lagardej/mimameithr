using Friflo.Engine.ECS;
using Kjarni.Brunnr.Command;
using Kjarni.Brunnr.Engine.Cell;

namespace Kjarni.Nornir.Hlothyn.Tectonics.StagnantLid;

/// <summary>Handles <see cref="SetTectonicsStagnantLid" /> commands against the entity store.</summary>
public class SetTectonicsStagnantLidHandler(EntityStore store) : ICommandHandler<SetTectonicsStagnantLid>
{
    /// <inheritdoc />
    public void Handle(SetTectonicsStagnantLid command)
    {
        var entity = store.GetEntityById(command.Id);
        RegimeInvariant.RemoveAllRegimeComponents(store, entity);
        entity.AddComponent(new TectonicsRegimeC { Regime = Regime.StagnantLid });

        store.Query<CellParentRefC>().ForEachEntity((ref parentRef, cellEntity) =>
        {
            if (parentRef.Parent != entity)
            {
                return;
            }

            cellEntity.AddComponent(new TectonicsStagnantLidC());
        });
    }
}
