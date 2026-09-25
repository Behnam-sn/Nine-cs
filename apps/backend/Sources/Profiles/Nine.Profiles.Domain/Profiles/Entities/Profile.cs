using Nine.Identities.Domain.Contracts.Users.ValueObjects;
using Nine.Profiles.Domain.Contracts.Profiles.Events;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.SharedKernel.Abstractions.AggregateRoots;
using Nine.SharedKernel.Abstractions.Events;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Profiles.Domain.Profiles.Entities;

public sealed class Profile : EventSourcedAggregateRoot<ProfileId>
{
    private Profile()
    {
    }
    
    public Profile(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            ApplyDomainEvent(domainEvent);
        }
    }

    public ProfileId ProfileId { get; private set; }
    public ProfileName Name { get; private set; }
    public ProfileHandle Handle { get; private set; }
    public ProfileAvatar? Avatar { get; private set; }
    public ProfileBio Bio { get; private set; }
    public UserId OwnerId { get; private set; }
    
    private void ApplyDomainEvent(ProfileCreatedDomainEventV1 domainEvent)
    {
        ProfileId = domainEvent.ProfileId;
        Name = domainEvent.Name;
        Handle = domainEvent.Handle; 
        Avatar = domainEvent.Avatar;
        Bio = domainEvent.Bio;
        OwnerId = domainEvent.OwnerId;
    }

    public static Profile Create(ProfileName name, ProfileHandle handle, ProfileAvatar? avatar, ProfileBio bio, UserId ownerId)
    {
        var profile = new Profile();
        var profileCreatedDomainEvent = new ProfileCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            ProfileId: ProfileId.Create(),
            Name: name,
            Handle: handle,
            Avatar: avatar,
            Bio: bio,
            OwnerId: ownerId,
            Timestamp: DateTime.UtcNow
        );
        profile.RaiseDomainEvent(profileCreatedDomainEvent);
        profile.ApplyDomainEvent(profileCreatedDomainEvent);
        return profile;
    }
}