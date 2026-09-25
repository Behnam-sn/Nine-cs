using FluentAssertions;

using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Domain.Tests.Profiles.ValueObjects;

public sealed class ProfileBioTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("I love building products.")]
    [InlineData("A")]
    public void Create_ShouldSetValue(string value)
    {
        // Arrange

        // Act
        var bio = ProfileBio.Create(value);

        // Assert
        bio.Value.Should().Be(value.Trim());
    }

    [Theory]
    [InlineData("  I love building products.  ", "I love building products.")]
    [InlineData("  Designer  ", "Designer")]
    [InlineData("   ", "")]
    public void Create_ShouldTrimLeadingAndTrailingWhitespace(string input, string expected)
    {
        // Arrange

        // Act
        var bio = ProfileBio.Create(input);

        // Assert
        bio.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowProfileBioTooLongException()
    {
        // Arrange
        var longValue = new string('A', ProfileBio.MaxLength + 1);

        // Act
        var act = () => ProfileBio.Create(longValue);

        // Assert
        act.Should().Throw<ProfileBioTooLongException>();
    }
}
