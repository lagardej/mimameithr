namespace Kjarni.Kvasir.Foundation;

/// <summary>Deterministic hashing utilities for stable, seed-driven value generation.</summary>
public static class Hashing
{
    /// <summary>
    ///     FNV-1a inspired hash over four <see cref="ulong" /> inputs with avalanche finalization.
    ///     Deterministic for any given combination of inputs; suitable for seeded procedural generation.
    /// </summary>
    /// <param name="seed">Top-level seed, typically the simulation seed.</param>
    /// <param name="a">First discriminator (e.g. entity id).</param>
    /// <param name="b">Second discriminator.</param>
    /// <param name="c">Third discriminator.</param>
    /// <returns>A stable 32-bit hash value.</returns>
    public static uint StableHash(ulong seed, ulong a, ulong b, ulong c)
    {
        unchecked
        {
            var h = 2166136261;
            h = (h ^ (uint) seed) * 16777619;
            h = (h ^ (uint) (seed >> 32)) * 16777619;
            h = (h ^ (uint) a) * 16777619;
            h = (h ^ (uint) (a >> 32)) * 16777619;
            h = (h ^ (uint) b) * 16777619;
            h = (h ^ (uint) (b >> 32)) * 16777619;
            h = (h ^ (uint) c) * 16777619;
            h = (h ^ (uint) (c >> 32)) * 16777619;
            h ^= h >> 13;
            h *= 1274126177;
            h ^= h >> 16;
            return h;
        }
    }

    /// <summary>
    ///     Maps four <see cref="ulong" /> inputs to a value in [0, 1] via <see cref="StableHash" />.
    ///     Uses the lower 24 bits of the hash for uniform coverage of the unit interval.
    /// </summary>
    /// <param name="seed">Top-level seed.</param>
    /// <param name="a">First discriminator.</param>
    /// <param name="b">Second discriminator.</param>
    /// <param name="c">Third discriminator.</param>
    /// <returns>A deterministic value in [0, 1].</returns>
    public static double UnitIntervalHash(ulong seed, ulong a, ulong b, ulong c)
    {
        var hash = StableHash(seed, a, b, c);
        return (hash & 0xFFFFFFu) / 16777215d;
    }
}
