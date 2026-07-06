using Kjarni.Brunnr.Command;

namespace Kjarni.Nornir.Ginnungagap.Seed;

/// <summary>World generation seed.</summary>
/// <param name="Id">The entity id.</param>
/// <param name="Seed">Seed</param>
public record SetSeed(
    int Id,
    uint Seed
) : ICommand;
