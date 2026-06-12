using Brunnr.Component;
using Brunnr.Domain;

namespace Brunnr.Dynacore;

/// <summary>
///     Default <see cref="ICellIndexRegistry" /> implementation.
///     Caches one <see cref="CellIndex" /> per domain; rebuilds when the cell set changes or
///     <see cref="Invalidate" /> is called.
/// </summary>
public sealed class CellIndexRegistry : ICellIndexRegistry
{
    private readonly Dictionary<DomainId, (CellSet CellSet, CellIndex Index)> _cache = new();

    /// <inheritdoc />
    public CellIndex Get(DomainId domain, CellSet cells)
    {
        if (_cache.TryGetValue(domain, out var entry) && entry.CellSet == cells)
        {
            return entry.Index;
        }

        var sorted = cells.Cells.OrderBy(c => c.Value).ToList();
        var index = new CellIndex(sorted);
        _cache[domain] = (cells, index);
        return index;
    }

    /// <inheritdoc />
    public void Invalidate(DomainId domain) => _cache.Remove(domain);
}
