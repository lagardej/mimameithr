namespace Brunnr.Command;

/// <summary>A command.</summary>
public interface ICommand;

/// <summary>A command handler.</summary>
public interface ICommandHandler
{
    /// <summary>Handles the command.</summary>
    void Handle(ICommand command);
}

/// <summary>A command handler.</summary>
public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
{
    void ICommandHandler.Handle(ICommand command) => Handle((TCommand) command);

    /// <summary>Handles the command.</summary>
    void Handle(TCommand command);
}
