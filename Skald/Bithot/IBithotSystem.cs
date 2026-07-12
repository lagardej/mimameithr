namespace Bithot;

/// <summary>Contract for a Bithot system that performs render-side synchronisation work.</summary>
public interface IBithotSystem
{
    /// <summary>Advances the system by one Bithot frame.</summary>
    void Advance();
}