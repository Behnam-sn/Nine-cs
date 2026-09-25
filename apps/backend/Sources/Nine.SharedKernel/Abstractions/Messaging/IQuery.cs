using MediatR;

namespace Nine.SharedKernel.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
