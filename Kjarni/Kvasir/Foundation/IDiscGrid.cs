namespace Kjarni.Kvasir.Foundation;

/// <summary>
///     Planar extension of <see cref="IGrid" /> for flat disc-shaped bodies.
///     Adds coordinate-based lookup and 2D cell geometry using Cartesian coordinates
///     centred at the origin.
/// </summary>
public interface IDiscGrid : IGrid
{
    /// <summary>The maximum grid-ring distance from the centre cell to the rim of the disc.</summary>
    ulong MaxGridRing { get; }

    /// <summary>Resolves the cell containing the given planar coordinates at the given resolution.</summary>
    CellId CellAt(double x, double y, Resolution resolution);

    /// <inheritdoc cref="CellAt(double, double, Resolution)" />
    CellId CellAt(Vector2D position, Resolution resolution) => CellAt(position.X, position.Y, resolution);

    /// <summary>Returns the centre coordinate of a cell.</summary>
    Vector2D CenterOf(CellId cell);

    /// <summary>Returns the boundary polygon vertices of a cell in traversal order.</summary>
    Vector2D[] BoundaryOf(CellId cell);
}
