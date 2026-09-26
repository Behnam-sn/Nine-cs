using MediatR;

using Nine.Shared.Application.Abstractions.Messaging;

namespace Nine.Shared.Infrastructure.Messaging;

public sealed class CommandBus : ICommandBus
{
    private readonly ISender _sender;

    public CommandBus(ISender sender)
    {
        _sender = sender;
    }

    public Task Send(ICommand command, CancellationToken cancellationToken = default)
    {
        return _sender.Send(command, cancellationToken);
    }

    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        return _sender.Send(command, cancellationToken);
    }
}
