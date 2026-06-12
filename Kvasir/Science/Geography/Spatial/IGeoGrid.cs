using Kvasir.Science.Maths.Geometry.Partitioning;

namespace Kvasir.Science.Geography.Spatial;

/// <summary>
///     Geographic extension of <see cref="IGrid" /> for spherical discrete global grids.
///     Adds coordinate-based lookup and geographic cell geometry using latitude/longitude.
/// </summary>
public interface IGeoGrid : IGrid
{
    /// <summary>Resolves the cell at the given coordinates and resolution.</summary>
    CellId CellAt(double latDeg, double lngDeg, Resolution resolution);

    /// <inheritdoc cref="CellAt(double, double, Resolution)" />
    CellId CellAt(LatLng latLng, Resolution resolution) => CellAt(latLng.Lat, latLng.Lng, resolution);

    /// <summary>Returns the centre coordinate of a cell.</summary>
    LatLng CenterOf(CellId cell);

    /// <summary>Returns the boundary polygon vertices of a cell in traversal order.</summary>
    LatLng[] BoundaryOf(CellId cell);
}
