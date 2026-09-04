using Nine.Identities.Domain.Accounts.Entities;

namespace Nine.Identities.Domain.Accounts.Repositories;

public interface IAccountCommandRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
}
