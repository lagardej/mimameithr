using Kvasir;

namespace Nornir.Ginnungagap.Seed;

/// <summary>Provides deterministic random seeds derived from the world generation seed.</summary>
public sealed class RandomProvider
{
    private bool _isSet;
    private uint _seed;

    /// <summary>Sets world generation seed used to derive deterministic per-system seeds.</summary>
    public void SetSeed(uint seed)
    {
        _seed = seed;
        _isSet = true;
    }

    /// <summary>Returns world generation seed.</summary>
    /// <exception cref="InvalidOperationException">Thrown when seed has not been set.</exception>
    public uint GetSeed() => _isSet ? _seed : throw new InvalidOperationException("World generation seed is not set.");

    /// <summary>Derives deterministic seed from world seed and up to three scope values.</summary>
    public uint DeriveSeed(ulong a, ulong b = 0, ulong c = 0) => Hashing.StableHash(GetSeed(), a, b, c);

    /// <summary>Creates deterministic random stream scoped by up to three values.</summary>
    public StableRandom CreateStream(ulong a, ulong b = 0, ulong c = 0) => new(DeriveSeed(a, b, c));
}
