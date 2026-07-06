namespace Kjarni.Kvasir.Foundation;

/// <summary>A closed interval [Min, Max].</summary>
public readonly record struct Range<T>(T Min, T Max) where T : struct;

/// <summary>Provides methods for mapping a range between specified minimum and maximum values.</summary>
public static class Scaling
{
    /// <summary>Gets a range spanning from 1 to 10.</summary>
    public static Range<uint> Range10 => new(1, 10);

    /// <summary>Gets a range spanning from 1 to 100.</summary>
    public static Range<uint> Range100 => new(1, 100);

    extension(Range<uint> range)
    {
        /// <summary>Maps a scale within a given range to a linear range between a specified minimum and maximum value.</summary>
        public double LinearScale(uint value, double min, double max)
        {
            var clampedScale = Math.Clamp(value, range.Min, range.Max);
            return min + ((max - min) * (clampedScale - range.Min) / (range.Max - range.Min));
        }

        /// <summary>Maps a scale within a given range to an exponential range between a specified minimum and maximum value.</summary>
        public double ExponentialScale(uint scale, double min, double max)
        {
            var clampedScale = Math.Clamp(scale, range.Min, range.Max);
            return min * Math.Pow(max / min,
                (clampedScale - range.Min) / (double) (range.Max - range.Min));
        }
    }
}
