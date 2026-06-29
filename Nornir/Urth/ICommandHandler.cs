namespace Nornir.Urth;

/// <summary>Contract for a command endpoint in the Urth generation engine.</summary>
public interface ICommandHandler
{
    /// <summary>Applies the command to the entity with the given <paramref name="id" />.</summary>
    void Handle(int id, object command);
}
