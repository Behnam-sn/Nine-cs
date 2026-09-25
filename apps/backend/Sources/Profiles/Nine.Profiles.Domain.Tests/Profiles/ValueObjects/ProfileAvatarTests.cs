using FluentAssertions;

using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Domain.Tests.Profiles.ValueObjects;

public sealed class ProfileAvatarTests
{
    [Theory]
    [InlineData("avatars/profile.png", "image/png")]
    [InlineData("avatars/profile.jpg", "image/jpeg")]
    [InlineData("avatars/profile.webp", "image/webp")]
    [InlineData("avatars/profile.gif", "image/gif")]
    public void Create_ShouldSetValues(string objectKey, string mediaType)
    {
        var avatar = ProfileAvatar.Create(objectKey, mediaType);

        avatar.ObjectKey.Should().Be(objectKey);
        avatar.MediaType.Should().Be(mediaType);
    }

    [Fact]
    public void Create_ShouldTrimValues()
    {
        var avatar = ProfileAvatar.Create(" avatars/profile.png ", " IMAGE/PNG ");

        avatar.ObjectKey.Should().Be("avatars/profile.png");
        avatar.MediaType.Should().Be("image/png");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceObjectKey_ShouldThrowProfileAvatarObjectKeyCannotBeEmptyException(string objectKey)
    {
        var act = () => ProfileAvatar.Create(objectKey, "image/png");

        act.Should().Throw<ProfileAvatarObjectKeyCannotBeEmptyException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    public void Create_WithInvalidMediaType_ShouldThrowProfileAvatarInvalidMediaTypeException(string mediaType)
    {
        var act = () => ProfileAvatar.Create("avatars/profile.png", mediaType);

        act.Should().Throw<ProfileAvatarInvalidMediaTypeException>();
    }
}
