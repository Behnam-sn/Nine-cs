using MediatR;

namespace Nine.Shared.Application.Abstractions.Messaging;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
