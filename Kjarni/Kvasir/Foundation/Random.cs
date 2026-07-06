namespace Kjarni.Kvasir.Foundation;

/// <summary>
///     Deterministic pseudo-random number generator based on a 32-bit XOR-shift algorithm.
///     Stateful: each call to <see cref="NextDouble" /> advances internal state.
///     Not safe to share across concurrent calls.
/// </summary>
/// <param name="seed">Initial state seed. If zero, defaults to a fixed non-zero constant.</param>
public sealed class StableRandom(uint seed)
{
    private uint _state = seed == 0 ? 0x9E3779B9u : seed;

    /// <summary>Advances the internal state and returns the next value in [0, 1].</summary>
    public double NextDouble()
    {
        _state ^= _state << 13;
        _state ^= _state >> 17;
        _state ^= _state << 5;
        return _state / 4294967295d;
    }
}
