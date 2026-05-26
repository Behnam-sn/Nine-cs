namespace Nine.SharedKernel.Abstractions.Messaging;

public interface ICommandBus
{
    Task Send(ICommand command, CancellationToken cancellationToken = default);

    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}