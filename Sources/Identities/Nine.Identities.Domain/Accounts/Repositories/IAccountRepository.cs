using Nine.Identities.Domain.Accounts.Entities;

namespace Nine.Identities.Domain.Accounts.Repositories;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
}
