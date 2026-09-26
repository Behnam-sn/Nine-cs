using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.Shared.Application.Abstractions.Messaging;

namespace Nine.Profiles.Application.Profiles.Commands.Create;

public sealed record CreateProfileCommandV1(
    string Name,
    string Handle,
    string? AvatarObjectKey,
    string? AvatarMediaType,
    string Bio,
    string OwnerId
) : ICommand<ProfileId>;
