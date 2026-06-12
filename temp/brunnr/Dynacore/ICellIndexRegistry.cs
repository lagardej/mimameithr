using Brunnr.Component;
using Brunnr.Domain;

namespace Brunnr.Dynacore;

/// <summary>
///     Manages <see cref="CellIndex" /> lifetime for <c>(Domain, CellSet)</c> pairs.
///     Responsibilities: index creation, rebuild on cell-set change, pooled field lifetime,
///     and stable ordinal mapping guarantees.
/// </summary>
public interface ICellIndexRegistry
{
    /// <summary>
    ///     Returns the <see cref="CellIndex" /> for the given <paramref name="domain" /> and
    ///     <paramref name="cells" />, creating or rebuilding it as needed.
    /// </summary>
    CellIndex Get(DomainId domain, CellSet cells);

    /// <summary>
    ///     Invalidates the cached index for <paramref name="domain" />, forcing a rebuild on
    ///     the next <see cref="Get" /> call. Call when the cell set for a domain changes.
    /// </summary>
    void Invalidate(DomainId domain);
}
