using JasperFx.Events;

using Marten.Events.Aggregation;

using Nine.Identities.Domain.Contracts.Accounts.Events;
using Nine.Identities.Infrastructure.Accounts.ReadModels;

namespace Nine.Identities.Infrastructure.Accounts.Projections;

public sealed class AccountPhoneNumberLookupProjection : SingleStreamProjection<AccountPhoneNumberLookup, Guid>
{
    public override AccountPhoneNumberLookup? Evolve(AccountPhoneNumberLookup? snapshot, Guid id, IEvent e)
    {
        return e.Data switch
        {
            AccountWithPasswordCreatedDomainEventV1 { PhoneNumber: { } phoneNumber } => new AccountPhoneNumberLookup
            {
                Id = id,
                PhoneNumber = phoneNumber.Value
            },
            AccountPhoneNumberChangedDomainEventV1 domainEvent => new AccountPhoneNumberLookup
            {
                Id = id,
                PhoneNumber = domainEvent.PhoneNumber.Value
            },
            _ => snapshot
        };
    }
}
