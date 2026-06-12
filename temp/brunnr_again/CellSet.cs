using Kvasir.Science.Maths.Geometry.Partitioning;

namespace Brunnr.Engine;

public record CellSet(IReadOnlySet<CellId> Cells)
{
    public static CellSet Global { get; } = new(new HashSet<CellId>());
}
