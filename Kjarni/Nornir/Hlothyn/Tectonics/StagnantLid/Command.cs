using Brunnr.Command;

namespace Nornir.Hlothyn.Tectonics.StagnantLid;

/// <summary>Dummy command to configure stagnant-lid tectonics of a planetary body.</summary>
/// <param name="Id">The entity id.</param>
public record SetTectonicsStagnantLid(
    int Id
) : ICommand;
