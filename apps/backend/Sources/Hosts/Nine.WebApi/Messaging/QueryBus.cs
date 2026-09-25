using MediatR;

using Nine.SharedKernel.Abstractions.Messaging;

namespace Nine.WebApi.Messaging;

public sealed class QueryBus : IQueryBus
{
    private readonly ISender _sender;

    public QueryBus(ISender sender)
    {
        _sender = sender;
    }

    public Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return _sender.Send(query, cancellationToken);
    }
}
