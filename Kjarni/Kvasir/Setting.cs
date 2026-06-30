namespace Kjarni.Kvasir;

/// <summary>
///     A validated, normalizable designer knob.
///     Stores a <see cref="uint" /> value within an explicit [<see cref="Min" />, <see cref="Max" />] range
///     and exposes it as a [0, 1] normalized <see cref="double" /> via <see cref="Normalized" />.
/// </summary>
public record Setting
{
    /// <param name="value">Designer-supplied value. Must be within [<paramref name="min" />, <paramref name="max" />].</param>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min" /> ≥ <paramref name="max" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="value" /> is outside [<paramref name="min" />
    ///     , <paramref name="max" />].
    /// </exception>
    public Setting(uint value, uint min, uint max)
    {
        if (min >= max)
        {
            throw new ArgumentException($"min ({min}) must be less than max ({max}).");
        }

        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Value {value} is outside [{min}, {max}].");
        }

        Value = value;
        Min = min;
        Max = max;
    }

    /// <summary>Raw value as supplied by the designer.</summary>
    public uint Value { get; }

    /// <summary>Inclusive lower bound.</summary>
    public uint Min { get; }

    /// <summary>Inclusive upper bound.</summary>
    public uint Max { get; }

    /// <summary>Value normalized to [0, 1] over [<see cref="Min" />, <see cref="Max" />].</summary>
    public double Normalized => (double) (Value - Min) / (Max - Min);
}

/// <summary>A <see cref="Setting" /> with a fixed [1, 10] range.</summary>
public record Setting10 : Setting
{
    /// <param name="value">Designer-supplied value. Must be within [1, 10].</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> is outside [1, 10].</exception>
    public Setting10(uint value) : base(value, 1, 10) { }
}

/// <summary>A <see cref="Setting" /> with a fixed [1, 100] range.</summary>
public record Setting100 : Setting
{
    /// <param name="value">Designer-supplied value. Must be within [1, 100].</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> is outside [1, 100].</exception>
    public Setting100(uint value) : base(value, 1, 100) { }
}
