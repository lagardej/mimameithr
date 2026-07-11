namespace Kjarni.Kvasir.Foundation;

/// <summary>A closed interval [Min, Max].</summary>
public readonly record struct Range<T>(T Min, T Max) where T : struct;

/// <summary>Provides common scale ranges.</summary>
public static class Scaling
{
    /// <summary>Gets a range spanning from 1 to 10.</summary>
    public static Range<uint> Range10 => new(1, 10);

    /// <summary>Gets a range spanning from 1 to 100.</summary>
    public static Range<uint> Range100 => new(1, 100);

    /// <summary>Gets a range spanning from 1 to 1000.</summary>
    public static Range<uint> Range1000 => new(1, 1000);
}

/// <summary>Maps a scale value linearly onto [<see cref="Min" />, <see cref="Max" />].</summary>
public readonly record struct LinearScale(Range<uint> Range, double Min, double Max)
{
    /// <summary>Maps <paramref name="value" /> from <see cref="Range" /> onto [<see cref="Min" />, <see cref="Max" />].</summary>
    public double Evaluate(uint value)
    {
        var clamped = Math.Clamp(value, Range.Min, Range.Max);
        return Min + ((Max - Min) * (clamped - Range.Min) / (Range.Max - Range.Min));
    }
}

/// <summary>Maps a scale value exponentially onto [<see cref="Min" />, <see cref="Max" />].</summary>
public readonly record struct ExponentialScale(Range<uint> Range, double Min, double Max)
{
    /// <summary>
    ///     Maps <paramref name="value" /> from <see cref="Range" /> exponentially onto [<see cref="Min" />,
    ///     <see cref="Max" />].
    /// </summary>
    public double Evaluate(uint value)
    {
        var clamped = Math.Clamp(value, Range.Min, Range.Max);
        return Min * Math.Pow(Max / Min, (clamped - Range.Min) / (double) (Range.Max - Range.Min));
    }
}

/// <summary>
///     Maps a scale value across multiple contiguous base-10 exponential zones.
///     Thresholds and exponents are fixed at construction — there is no implicit default,
///     so the zone boundaries are always sized correctly for the given <see cref="Range" />.
/// </summary>
public readonly record struct PiecewiseExponentialScale
{
    private readonly int[] _exponents;
    private readonly Range<uint> _range;
    private readonly uint[] _thresholds;

    /// <param name="range">The input scale's range.</param>
    /// <param name="exponents">
    ///     The base-10 integer exponents defining the boundaries of each zone. Length must be
    ///     thresholds.Length + 1.
    /// </param>
    /// <param name="thresholds">The upper scale boundaries for each zone, sized for <paramref name="range" />.</param>
    public PiecewiseExponentialScale(Range<uint> range, int[] exponents, uint[] thresholds)
    {
        if (thresholds.Length == 0 || exponents.Length != thresholds.Length + 1)
        {
            throw new ArgumentException("Exponents length must be exactly thresholds length + 1.");
        }

        _range = range;
        _exponents = exponents;
        _thresholds = thresholds;
    }

    /// <summary>Maps <paramref name="scale" /> through the configured zones to a value of the form 10^exponent.</summary>
    public double Evaluate(uint scale)
    {
        var clamped = Math.Clamp(scale, _range.Min, _range.Max);

        double lowerThreshold = _range.Min;
        double lowerExponent = _exponents[0];

        for (var i = 0; i < _thresholds.Length; i++)
        {
            double upperThreshold = _thresholds[i];
            double upperExponent = _exponents[i + 1];

            if (clamped <= upperThreshold || i == _thresholds.Length - 1)
            {
                upperThreshold = Math.Max(clamped, upperThreshold);

                var t = (clamped - lowerThreshold) / (upperThreshold - lowerThreshold);
                var exponent = lowerExponent + (t * (upperExponent - lowerExponent));
                return Math.Pow(10, exponent);
            }

            lowerThreshold = upperThreshold;
            lowerExponent = upperExponent;
        }

        return Math.Pow(10, _exponents[^1]);
    }

    /// <summary>Maps a quantity to its corresponding scale value by inverting the exponential mapping.</summary>
    public uint Inverse(double quantity)
    {
        var logQuantity = Math.Log10(quantity);

        double lowerThreshold = _range.Min;
        double lowerExponent = _exponents[0];

        for (var i = 0; i < _thresholds.Length; i++)
        {
            double upperThreshold = _thresholds[i];
            double upperExponent = _exponents[i + 1];

            if (logQuantity <= upperExponent || i == _thresholds.Length - 1)
            {
                var t = (logQuantity - lowerExponent) / (upperExponent - lowerExponent);
                var scale = lowerThreshold + (t * (upperThreshold - lowerThreshold));
                return (uint) Math.Clamp(Math.Round(scale), _range.Min, _range.Max);
            }

            lowerThreshold = upperThreshold;
            lowerExponent = upperExponent;
        }

        return _range.Max;
    }
}
