using Kvasir.Science.Geography.Spatial;

namespace Brunnr.Grid;

/// <summary>
///     Global grid instance. Initialized once at engine startup before any system runs.
///     The grid is stateless and deterministic — a singleton is appropriate here.
/// </summary>
public static class GeoGrid
{
    /// <summary>The active grid implementation.</summary>
    public static IGeoGrid Instance { get; private set; } = null!;

    /// <summary>Initializes the grid. Must be called once before the first tick.</summary>
    public static void Initialize(IGeoGrid grid) => Instance = grid;
}
