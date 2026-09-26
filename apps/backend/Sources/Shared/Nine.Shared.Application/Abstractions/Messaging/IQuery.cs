using MediatR;

namespace Nine.Shared.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
