using Kvasir.Grid;
using Xunit;

namespace Bifrost.Kart.Tests;

public sealed class KartTests
{
    private readonly global::Kart.Kart _grid = new();

    // Paris, R3
    private readonly Resolution _res = new(3);
    private CellId Cell => _grid.CellAt(48.8566, 2.3522, _res);

    [Fact]
    public void CellAt_ReturnsValidCell() =>
        Assert.True(_grid.IsValid(Cell));

    [Fact]
    public void ResolutionOf_MatchesRequested() =>
        Assert.Equal(_res, _grid.ResolutionOf(Cell));

    [Fact]
    public void CenterOf_ReturnsPlausibleCoordinates()
    {
        var center = _grid.CenterOf(Cell);

        Assert.InRange(center.Lat, 40.0, 55.0);
        Assert.InRange(center.Lng, -5.0, 10.0);
    }

    [Fact]
    public void BoundaryOf_ReturnsSixOrSevenVertices()
    {
        var boundary = _grid.BoundaryOf(Cell);

        Assert.InRange(boundary.Length, 6, 7);
    }

    [Fact]
    public void RootCells_Returns122Cells()
    {
        var roots = _grid.RootCells();

        Assert.Equal(122, roots.Length);
    }

    [Fact]
    public void ParentOf_ReturnsCoarserCell()
    {
        var parent = _grid.ParentOf(Cell, new Resolution(2));

        Assert.True(_grid.IsValid(parent));
        Assert.Equal(new Resolution(2), _grid.ResolutionOf(parent));
    }

    [Fact]
    public void ChildrenOf_ReturnsNonEmptySet()
    {
        var children = _grid.ChildrenOf(Cell, new Resolution(4));

        Assert.NotEmpty(children);
        Assert.All(children, c => Assert.True(_grid.IsValid(c)));
    }

    [Fact]
    public void CellsAtResolution_InvalidResolution_Throws()
    {
        var invalidResolution = new Resolution(_grid.MaxResolution.Value + 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => _grid.CellsAtResolution(invalidResolution).ToList());
    }

    [Fact]
    public void Disk_K0_ReturnsSingleCell() =>
        Assert.Equal([Cell], _grid.Disk(Cell, 0));

    [Fact]
    public void Disk_K1_ReturnsSevenCells() =>
        Assert.Equal(7, _grid.Disk(Cell, 1).Length);
}
