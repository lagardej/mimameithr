namespace Brunnr.Topology;

/// <summary>
///     Opaque cell identifier. The underlying representation is implementation-specific.
/// </summary>
public readonly record struct CellId(ulong Value);
