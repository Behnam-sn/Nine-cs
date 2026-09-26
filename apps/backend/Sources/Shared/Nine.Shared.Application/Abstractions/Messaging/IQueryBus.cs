namespace Nine.Shared.Application.Abstractions.Messaging;

public interface IQueryBus
{
    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}