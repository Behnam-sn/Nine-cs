using Nine.Identity.Domain.Contracts.Users.ValueObjects;
using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.Profiles.Domain.Profiles.Entities;
using Nine.Profiles.Domain.Profiles.Repositories;
using Nine.Profiles.Domain.Profiles.Services;
using Nine.Shared.Application.Abstractions.Messaging;

namespace Nine.Profiles.Application.Profiles.Commands.Create;

public sealed class CreateProfileCommandV1Handler : ICommandHandler<CreateProfileCommandV1, ProfileId>
{
    private readonly IProfileCommandRepository _profileCommandRepository;
    private readonly IProfileHandleUniquenessChecker _profileHandleUniquenessChecker;

    public CreateProfileCommandV1Handler(IProfileCommandRepository profileCommandRepository,
        IProfileHandleUniquenessChecker profileHandleUniquenessChecker)
    {
        _profileCommandRepository = profileCommandRepository;
        _profileHandleUniquenessChecker = profileHandleUniquenessChecker;
    }


    public async Task<ProfileId> Handle(CreateProfileCommandV1 request, CancellationToken cancellationToken)
    {
        var handle = ProfileHandle.Create(request.Handle);

        var isHandleTaken = await _profileHandleUniquenessChecker.IsTakenAsync(handle, cancellationToken);
        if (isHandleTaken)
        {
            throw new ProfileHandleAlreadyInUseException(handle);
        }

        var profile = Profile.Create(
            name: ProfileName.Create(request.Name),
            handle: ProfileHandle.Create(request.Handle),
            avatar: request.AvatarObjectKey is not null && request.AvatarMediaType is not null
                ? ProfileAvatar.Create(request.AvatarObjectKey, request.AvatarMediaType)
                : null,
            bio: ProfileBio.Create(request.Bio),
            ownerId: UserId.Parse(request.OwnerId)
        );

        await _profileCommandRepository.AddAsync(profile, cancellationToken);

        return profile.ProfileId;
    }
}
