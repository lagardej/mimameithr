using Kvasir.Formal.Maths.Geometry.Partitioning;
using Kvasir.Natural.Physical.Geodesy;

namespace Nornir.Tests;

public sealed class StubGrid : IGeodesicGrid
{
    public static readonly StubGrid Instance = new();

    public LatLng CenterOf(CellId cell) => new(0, 0);

    public CellId CellAt(double latDeg, double lngDeg, Resolution resolution) => default;

    public LatLng[] BoundaryOf(CellId cell) => [];

    public CellId[] RootCells() => [];

    public IEnumerable<CellId> CellsAtResolution(Resolution resolution) => [];

    public Resolution ResolutionOf(CellId cell) => default;

    public bool IsValid(CellId cell) => false;

    public CellId ParentOf(CellId cell, Resolution parentResolution) => default;

    public CellId[] ChildrenOf(CellId cell, Resolution childResolution) => [];

    public Resolution InferResolution<T>(Dictionary<CellId, T> cells, Resolution fallback) => fallback;

    public CellId[] Disk(CellId cell, int k) => [];

    public IEnumerable<CellId> GridRing(CellId center, int k) => [];
}
