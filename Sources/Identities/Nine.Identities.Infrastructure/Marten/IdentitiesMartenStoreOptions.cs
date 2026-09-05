using JasperFx.Events.Projections;

using Marten;
using Marten.Schema;

using Nine.Identities.Domain.Contracts.Accounts.Events;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.Identities.Infrastructure.Accounts.Projections;
using Nine.Identities.Infrastructure.Accounts.ReadModels;
using Nine.Identities.Infrastructure.Marten.Serialization;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

using Weasel.Core;

namespace Nine.Identities.Infrastructure.Marten;

public static class IdentitiesMartenStoreOptions
{
    public static void Configure(StoreOptions options, string connectionString)
    {
        options.Connection(connectionString);
        options.DatabaseSchemaName = "identities";
        options.Events.DatabaseSchemaName = "identities";

        options.UseSystemTextJsonForSerialization(
            enumStorage: EnumStorage.AsString,
            casing: Casing.CamelCase,
            configure: json =>
            {
                json.Converters.Add(new ValueObjectJsonConverter<AccountId, Guid>(static id => id.Value, AccountId.From));
                json.Converters.Add(new ValueObjectJsonConverter<CredentialId, Guid>(static id => id.Value, CredentialId.From));
                json.Converters.Add(new ValueObjectJsonConverter<DomainEventId, Guid>(static id => id.Value, DomainEventId.Parse));
                json.Converters.Add(new ValueObjectJsonConverter<EmailAddress, string>(static email => email.Value, EmailAddress.Create));
                json.Converters.Add(new ValueObjectJsonConverter<HashedSecret, string>(static secret => secret.Value, HashedSecret.Create));
                json.Converters.Add(new ValueObjectJsonConverter<PhoneNumber, string>(static phone => phone.Value, PhoneNumber.Create));
            });

        var eventTypes = typeof(AccountWithPasswordCreatedDomainEventV1).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IDomainEvent).IsAssignableFrom(type));

        foreach (var eventType in eventTypes)
        {
            options.Events.AddEventType(eventType);
        }

        // Account Email Address Lookup
        options.Schema.For<AccountEmailAddressLookup>()
            .UniqueIndex(UniqueIndexType.DuplicatedField, lookup => lookup.EmailAddress);

        options.Projections.Add<AccountEmailAddressLookupProjection>(ProjectionLifecycle.Inline);
        
        // Account Phone Number Lookup
        options.Schema.For<AccountPhoneNumberLookup>()
            .UniqueIndex(UniqueIndexType.DuplicatedField, lookup => lookup.PhoneNumber);
        
        options.Projections.Add<AccountPhoneNumberLookupProjection>(ProjectionLifecycle.Inline);
    }
}
