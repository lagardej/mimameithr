namespace Kvasir.Grid;

/// <summary>
///     Grid resolution level. R0 is the coarsest level; higher values are finer.
///     The valid range and performance characteristics are backend-specific.
/// </summary>
public readonly record struct Resolution(uint Value)
{
    /// <summary>Coarsest resolution. The only level guaranteed to exist for all backends.</summary>
    public static readonly Resolution R0 = new(0);
}
