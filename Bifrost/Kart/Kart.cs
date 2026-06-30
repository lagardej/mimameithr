using Kjarni.Kvasir.Formal.Maths.Geometry.Partitioning;
using Kjarni.Kvasir.Natural.Physical.Geodesy;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bifrost.Kart;

/// <summary>
///     <see cref="IGeodesicGrid" /> implementation backed by the h3o Rust library via P/Invoke.
/// </summary>
public sealed partial class Kart : IGeodesicGrid
{
    #region Helpers

    private void GuardResolution(Resolution resolution)
    {
        if (resolution.Value <= MaxResolution.Value)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(resolution),
            resolution.Value,
            $"Resolution {resolution.Value} exceeds the maximum supported level ({MaxResolution.Value}).");
    }

    #endregion

    #region IGeodesicGrid

    /// <inheritdoc />
    public CellId CellAt(double latDeg, double lngDeg, Resolution resolution)
    {
        GuardResolution(resolution);
        return new CellId(geogrid_cell_at(latDeg, lngDeg, resolution.Value));
    }

    /// <inheritdoc />
    public LatLng CenterOf(CellId cell) =>
        geogrid_center_of(cell.Value, out var lat, out var lng)
            ? new LatLng(lat, lng)
            : throw new ArgumentException($"Invalid cell: {cell.Value}", nameof(cell));

    /// <inheritdoc />
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

    #endregion

    #region IGrid

    /// <inheritdoc />
    public Resolution MaxResolution { get; } = new(15);

    /// <inheritdoc />
    public CellId[] RootCells()
    {
        var len = geogrid_root_cells_len();
        var buf = new ulong[len];
        geogrid_root_cells(buf, len);
        return buf.Select(v => new CellId(v)).ToArray();
    }

    /// <inheritdoc />
    public IEnumerable<CellId> CellsAtResolution(Resolution resolution)
    {
        GuardResolution(resolution);
        var len = geogrid_cells_at_resolution_len(resolution.Value);
        var buf = new ulong[len];
        _ = geogrid_cells_at_resolution(resolution.Value, buf, len);
        return buf.Select(v => new CellId(v));
    }

    /// <inheritdoc />
    public Resolution ResolutionOf(CellId cell) =>
        new(geogrid_resolution_of(cell.Value));

    /// <inheritdoc />
    public bool IsValid(CellId cell) =>
        geogrid_is_valid(cell.Value);

    /// <inheritdoc />
    public CellId ParentOf(CellId cell, Resolution parentResolution)
    {
        GuardResolution(parentResolution);
        return new CellId(geogrid_parent_of(cell.Value, parentResolution.Value));
    }

    /// <inheritdoc />
    public CellId[] ChildrenOf(CellId cell, Resolution childResolution)
    {
        GuardResolution(childResolution);
        var len = geogrid_children_len(cell.Value, childResolution.Value);
        if (len == -1)
        {
            throw new ArgumentException($"Invalid cell: {cell.Value}", nameof(cell));
        }

        var buf = new ulong[len];
        _ = geogrid_children_of(cell.Value, childResolution.Value, buf, len);
        return buf.Select(v => new CellId(v)).ToArray();
    }

    /// <inheritdoc />
    public Resolution InferResolution<T>(Dictionary<CellId, T> cells, Resolution fallback)
    {
        foreach (var k in cells.Keys)
        {
            return ResolutionOf(k);
        }

        return fallback;
    }

    /// <inheritdoc />
    public CellId[] Disk(CellId cell, int k)
    {
        var len = geogrid_disk_len(cell.Value, k);
        if (len == -1)
        {
            throw new ArgumentException($"Invalid cell: {cell.Value}", nameof(cell));
        }

        var buf = new ulong[len];
        _ = geogrid_disk(cell.Value, k, buf, len);
        return buf.Select(v => new CellId(v)).ToArray();
    }

    /// <inheritdoc />
    public IEnumerable<CellId> GridRing(CellId center, int k)
    {
        if (k == 0)
        {
            return [center];
        }

        var inner = new HashSet<CellId>(Disk(center, k - 1));
        return Disk(center, k).Where(c => !inner.Contains(c));
    }

    #endregion

    #region P/Invoke

    private const string KartLib = "kart";

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ulong geogrid_cell_at(double lat, double lng, uint res);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial uint geogrid_resolution_of(ulong cell);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool geogrid_is_valid(ulong cell);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool geogrid_center_of(ulong cell, out double lat, out double lng);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_boundary_of(ulong cell, [Out] double[] outBuffer);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ulong geogrid_parent_of(ulong cell, uint parentRes);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_children_len(ulong cell, uint childRes);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_children_of(ulong cell, uint childRes, [Out] ulong[] outBuffer, int len);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_root_cells_len();

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void geogrid_root_cells([Out] ulong[] outBuffer, int len);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_cells_at_resolution_len(uint res);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_cells_at_resolution(uint res, [Out] ulong[] outBuffer, int len);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_disk_len(ulong cell, int k);

    [LibraryImport(KartLib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int geogrid_disk(ulong cell, int k, [Out] ulong[] outBuffer, int len);

    #endregion
}
