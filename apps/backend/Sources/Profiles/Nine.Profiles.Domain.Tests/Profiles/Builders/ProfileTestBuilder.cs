using Nine.Identity.Domain.Contracts.Users.ValueObjects;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.Profiles.Domain.Profiles.Entities;

namespace Nine.Profiles.Domain.Tests.Profiles.Builders;

public sealed class ProfileTestBuilder
{
    public const string DefaultNameValue = "John Doe";
    public const string DefaultHandleValue = "johndoe";
    public const string DefaultAvatarObjectKeyValue = "avatars/profile.png";
    public const string DefaultAvatarMediaTypeValue = "image/png";
    public const string DefaultBioValue = "I love building products.";

    public static readonly ProfileName DefaultName = ProfileName.Create(DefaultNameValue);
    public static readonly ProfileHandle DefaultHandle = ProfileHandle.Create(DefaultHandleValue);
    public static readonly ProfileAvatar DefaultAvatar = ProfileAvatar.Create(DefaultAvatarObjectKeyValue, DefaultAvatarMediaTypeValue);
    public static readonly ProfileBio DefaultBio = ProfileBio.Create(DefaultBioValue);
    public static readonly UserId DefaultOwnerId = UserId.Create();

    private ProfileName _name;
    private ProfileHandle _handle;
    private ProfileAvatar? _avatar;
    private ProfileBio _bio;
    private UserId _ownerId;

    public ProfileTestBuilder WithName(ProfileName name)
    {
        _name = name;
        return this;
    }

    public ProfileTestBuilder WithHandle(ProfileHandle handle)
    {
        _handle = handle;
        return this;
    }

    public ProfileTestBuilder WithAvatar(ProfileAvatar avatar)
    {
        _avatar = avatar;
        return this;
    }

    public ProfileTestBuilder WithBio(ProfileBio bio)
    {
        _bio = bio;
        return this;
    }

    public ProfileTestBuilder WithOwnerId(UserId ownerId)
    {
        _ownerId = ownerId;
        return this;
    }

    public ProfileTestBuilder WithRequiredParameters()
    {
        WithName(DefaultName);
        WithHandle(DefaultHandle);
        WithBio(DefaultBio);
        WithOwnerId(DefaultOwnerId);
        return this;
    }

    public ProfileTestBuilder WithOptionalParameters()
    {
        WithAvatar(DefaultAvatar);
        return this;
    }

    public Profile Build()
    {
        var profile = Profile.Create(
            name: _name,
            handle: _handle,
            avatar: _avatar,
            bio: _bio,
            ownerId: _ownerId
        );
        return profile;
    }
}