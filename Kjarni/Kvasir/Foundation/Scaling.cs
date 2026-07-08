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


        /// <summary>
        /// Maps a scale value across multiple contiguous base-10 exponential zones.
        /// </summary>
        /// <param name="scale">The input slider value.</param>
        /// <param name="exponents">The base-10 integer exponents defining the boundaries of each zone. Length must be thresholds.Length + 1.</param>
        /// <param name="thresholds">The upper slider boundaries for each zone. Default to [40u, 70u, 100u]</param>
        public double PiecewiseExponentialScale(uint scale, ReadOnlySpan<int> exponents,
            ReadOnlySpan<uint> thresholds = default)
        {
            if (thresholds.IsEmpty)
            {
                thresholds = [40u, 70u, 100u];
            }

            if (thresholds.Length == 0 || exponents.Length != thresholds.Length + 1)
            {
                throw new ArgumentException("Exponents length must be exactly thresholds length + 1.");
            }

            var clampedScale = Math.Clamp(scale, range.Min, range.Max);

            double lowerThreshold = range.Min;
            double lowerExponent = exponents[0];

            for (int i = 0; i < thresholds.Length; i++)
            {
                double upperThreshold = thresholds[i];
                double upperExponent = exponents[i + 1];

                if (clampedScale <= upperThreshold || i == thresholds.Length - 1)
                {
                    upperThreshold = Math.Max(clampedScale, upperThreshold);

                    double t = (clampedScale - lowerThreshold) / (upperThreshold - lowerThreshold);
                    double exponent = lowerExponent + t * (upperExponent - lowerExponent);
                    return Math.Pow(10, exponent);
                }

                lowerThreshold = upperThreshold;
                lowerExponent = upperExponent;
            }

            return Math.Pow(10, exponents[^1]);
        }
    }
}
