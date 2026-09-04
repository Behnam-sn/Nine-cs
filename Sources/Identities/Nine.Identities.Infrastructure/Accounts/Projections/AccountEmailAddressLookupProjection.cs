using Marten.Events.Aggregation;

using Nine.Identities.Domain.Contracts.Accounts.Events;
using Nine.Identities.Infrastructure.Accounts.ReadModels;

namespace Nine.Identities.Infrastructure.Accounts.Projections;

public sealed class AccountEmailAddressLookupProjection : SingleStreamProjection<AccountEmailAddressLookup, Guid>
{
    public AccountEmailAddressLookup Create(AccountWithPasswordCreatedDomainEventV1 domainEvent)
    {
        return new AccountEmailAddressLookup
        {
            Id = domainEvent.AccountId.Value,
            EmailAddress = domainEvent.EmailAddress.Value
        };
    }

    public void Apply(AccountEmailAddressChangedDomainEventV1 domainEvent, AccountEmailAddressLookup lookup)
    {
        lookup.EmailAddress = domainEvent.EmailAddress.Value;
    }
}
