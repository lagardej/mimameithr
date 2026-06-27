namespace Kvasir.Formal.Maths.Geometry.Partitioning;

/// <summary>
///     Opaque cell identifier. The underlying representation is implementation-specific.
/// </summary>
[Module("Formal/Maths/Geometry/Partitioning", "Opaque cell identifier for discrete grid partitioning.")]
public readonly record struct CellId(ulong Value);
