using JasperFx.Events.Projections;
using Marten;
using Marten.Schema;
using Nine.Identity.Domain.Contracts.Users.ValueObjects;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.Profiles.Infrastructure.Profiles.Projections;
using Nine.Profiles.Infrastructure.Profiles.ReadModels;
using Nine.Profiles.Infrastructure.Profiles.Serialization;
using Nine.Shared.Domain.Abstractions.Events;
using Nine.Shared.Domain.Abstractions.ValueObjects;
using Nine.Shared.Infrastructure.Marten.Serialization;
using Weasel.Core;

namespace Nine.Profiles.Infrastructure.Profiles;

public static class ProfilesMartenStoreOptions
{
    public static void Configure(StoreOptions options, string connectionString)
    {
        options.Connection(connectionString);
        options.DatabaseSchemaName = "profiles";
        options.Events.DatabaseSchemaName = "profiles";

        options.UseSystemTextJsonForSerialization(
            enumStorage: EnumStorage.AsString,
            casing: Casing.CamelCase,
            configure: json =>
            {
                json.Converters.Add(new ValueObjectJsonConverter<ProfileId, Guid>(static id => id.Value, ProfileId.From));
                json.Converters.Add(new ValueObjectJsonConverter<ProfileName, string>(static name => name.Value, ProfileName.Create));
                json.Converters.Add(new ValueObjectJsonConverter<ProfileHandle, string>(static handle => handle.Value, ProfileHandle.Create));
                json.Converters.Add(new ValueObjectJsonConverter<ProfileBio, string>(static bio => bio.Value, ProfileBio.Create));
                json.Converters.Add(new ProfileAvatarJsonConverter());
                json.Converters.Add(new ValueObjectJsonConverter<UserId, Guid>(static id => id.Value, UserId.From));
                json.Converters.Add(new ValueObjectJsonConverter<DomainEventId, Guid>(static id => id.Value, DomainEventId.Parse));
            });

        var eventTypes = Domain.Contracts.AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IDomainEvent).IsAssignableFrom(type));

        foreach (var eventType in eventTypes)
        {
            options.Events.AddEventType(eventType);
        }

        // Account Email Address Lookup
        options.Schema.For<ProfileHandleLookup>()
            .UniqueIndex(UniqueIndexType.DuplicatedField, lookup => lookup.Handle);

        options.Projections.Add<ProfileHandleLookupProjection>(ProjectionLifecycle.Inline);
    }
}
