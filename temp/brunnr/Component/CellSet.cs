using Brunnr.Topology;

namespace Brunnr.Component;

/// <summary>The set of cells a component operates on for a given tick.</summary>
public record CellSet(IReadOnlySet<CellId> Cells)
{
    public static CellSet Global { get; } = new(new HashSet<CellId>());
}
