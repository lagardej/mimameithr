using Kvasir.Formal.Maths.Geometry.Partitioning;
using Kvasir.Natural.Physical.Geodesy;
using System.Runtime.InteropServices;

namespace GeogridH3o;

/// <summary>
///     <see cref="IGeodesicGrid" /> implementation backed by the h3o Rust library via P/Invoke.
/// </summary>
public sealed class GeodesicGridH3O : IGeodesicGrid
{
    // --- IGeoGrid ------------------------------------------------------------

    public CellId CellAt(double latDeg, double lngDeg, Resolution resolution) =>
        new(geogrid_cell_at(latDeg, lngDeg, resolution.Value));

    public LatLng CenterOf(CellId cell)
    {
        geogrid_center_of(cell.Value, out var lat, out var lng);
        return new LatLng(lat, lng);
    }

    public LatLng[] BoundaryOf(CellId cell)
    {
        var buf = new double[10 * 2];
        var count = geogrid_boundary_of(cell.Value, buf);
        var result = new LatLng[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = new LatLng(buf[i * 2], buf[(i * 2) + 1]);
        }

        return result;
    }

    // --- IGrid ---------------------------------------------------------------

    public CellId[] RootCells()
    {
        var len = geogrid_root_cells_len();
        var buf = new ulong[len];
        geogrid_root_cells(buf, len);
        return buf.Select(v => new CellId(v)).ToArray();
    }

    public IEnumerable<CellId> CellsAtResolution(Resolution resolution)
    {
        var len = geogrid_cells_at_resolution_len(resolution.Value);
        var buf = new ulong[len];
        geogrid_cells_at_resolution(resolution.Value, buf, len);
        return buf.Select(v => new CellId(v));
    }

    public Resolution ResolutionOf(CellId cell) =>
        new(geogrid_resolution_of(cell.Value));

    public bool IsValid(CellId cell) =>
        geogrid_is_valid(cell.Value);

    public CellId ParentOf(CellId cell, Resolution parentResolution) =>
        new(geogrid_parent_of(cell.Value, parentResolution.Value));

    public CellId[] ChildrenOf(CellId cell, Resolution childResolution)
    {
        var len = geogrid_children_len(cell.Value, childResolution.Value);
        var buf = new ulong[len];
        geogrid_children_of(cell.Value, childResolution.Value, buf, len);
        return buf.Select(v => new CellId(v)).ToArray();
    }

    public Resolution InferResolution<T>(Dictionary<CellId, T> cells, Resolution fallback)
    {
        foreach (var k in cells.Keys)
        {
            return ResolutionOf(k);
        }

        return fallback;
    }

    public CellId[] Disk(CellId cell, int k)
    {
        var len = geogrid_disk_len(cell.Value, k);
        var buf = new ulong[len];
        geogrid_disk(cell.Value, k, buf, len);
        return buf.Select(v => new CellId(v)).ToArray();
    }

    public IEnumerable<CellId> GridRing(CellId center, int k)
    {
        if (k == 0)
        {
            return [center];
        }

        var inner = new HashSet<CellId>(Disk(center, k - 1));
        return Disk(center, k).Where(c => !inner.Contains(c));
    }
    // --- P/Invoke ------------------------------------------------------------

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong geogrid_cell_at(double lat, double lng, int res);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_resolution_of(ulong cell);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool geogrid_is_valid(ulong cell);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool geogrid_center_of(ulong cell, out double lat, out double lng);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_boundary_of(ulong cell, [Out] double[] out_);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong geogrid_parent_of(ulong cell, int parentRes);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_children_len(ulong cell, int childRes);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_children_of(ulong cell, int childRes, [Out] ulong[] out_, int len);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_root_cells_len();

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern void geogrid_root_cells([Out] ulong[] out_, int len);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_cells_at_resolution_len(int res);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_cells_at_resolution(int res, [Out] ulong[] out_, int len);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_disk_len(ulong cell, int k);

    [DllImport("seidr", CallingConvention = CallingConvention.Cdecl)]
    private static extern int geogrid_disk(ulong cell, int k, [Out] ulong[] out_, int len);
}
