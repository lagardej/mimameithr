namespace Brunnr.Topology;

/// <summary>Math operations over per-cell scalar fields.</summary>
public static class CellField
{
    /// <summary>
    ///     Decays all values in <paramref name="field" /> toward zero by <paramref name="amount" /> each tick.
    ///     Values are clamped to [0, 1] after decay.
    /// </summary>
    /// <param name="field">Mutable per-cell scalar field.</param>
    /// <param name="amount">Decay amount in [0, 1]; 0 = no decay, 1 = full reset.</param>
    public static void Relax(Dictionary<CellId, double> field, double amount)
    {
        var decay = Math.Clamp(1d - amount, 0d, 1d);
        var keys = field.Keys.ToArray();
        foreach (var key in keys)
        {
            field[key] = Math.Clamp(field[key] * decay, 0d, 1d);
        }
    }

    /// <summary>
    ///     Returns the mean value across all cells in <paramref name="field" />, or 0 if the field is empty.
    /// </summary>
    /// <param name="field">Per-cell scalar field.</param>
    public static double Mean(IReadOnlyDictionary<CellId, double> field)
        => field.Count == 0 ? 0d : field.Values.Average();
}
