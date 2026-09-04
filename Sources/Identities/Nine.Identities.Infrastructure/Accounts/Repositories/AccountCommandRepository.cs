using Marten;

using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Accounts.Repositories;

namespace Nine.Identities.Infrastructure.Accounts.Repositories;

public sealed class AccountCommandRepository : IAccountCommandRepository
{
    private readonly IDocumentSession _session;

    public AccountCommandRepository(IDocumentSession session)
    {
        _session = session;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        var uncommittedEvents = account.DomainEvents.ToArray();
        if (uncommittedEvents.Length == 0)
        {
            return;
        }

        _session.Events.StartStream<Account>(account.AccountId.Value, uncommittedEvents);
        await _session.SaveChangesAsync(cancellationToken);
        account.ClearDomainEvents();
    }
}
