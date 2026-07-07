using Kjarni.Brunnr.Command;

namespace Kjarni.Nornir.Ginnungagap.Seed;

/// <summary>World generation seed.</summary>
/// <param name="Seed">Seed</param>
public record SetSeed(
    uint Seed
) : ICommand;
