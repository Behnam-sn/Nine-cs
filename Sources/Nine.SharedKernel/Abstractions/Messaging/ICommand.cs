using MediatR;

namespace Nine.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Represents the command interface.
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Represents the command interface.
/// </summary>
/// <typeparam name="TResponse">The command response type.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
}
