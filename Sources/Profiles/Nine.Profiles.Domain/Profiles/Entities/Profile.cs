using Nine.Identities.Domain.Contracts.Users.ValueObjects;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.SharedKernel.Abstractions.AggregateRoots;

namespace Nine.Profiles.Domain.Profiles.Entities;

public sealed class Profile : EventSourcedAggregateRoot<ProfileId>
{
    public ProfileId ProfileId { get; private set; }
    public ProfileName Name { get; private set; }
    public ProfileHandle Handle { get; private set; }
    public ProfileAvatar? Avatar { get; private set; }
    public ProfileBio Bio { get; private set; }
    public UserId OwnerId { get; private set; }
}