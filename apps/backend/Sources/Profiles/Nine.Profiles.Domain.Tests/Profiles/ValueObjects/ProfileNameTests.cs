using FluentAssertions;
using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Domain.Tests.Profiles.ValueObjects;

public sealed class ProfileNameTests
{
    [Theory]
    [InlineData("John Doe")]
    [InlineData("Alice")]
    [InlineData("A")]
    public void Create_ShouldSetValue(string value)
    {
        // Arrange

        // Act
        var name = ProfileName.Create(value);

        // Assert
        name.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowProfileNameCannotBeEmptyException(string invalidValue)
    {
        // Arrange

        // Act
        var act = () => ProfileName.Create(invalidValue);

        // Assert
        act.Should().Throw<ProfileNameCannotBeEmptyException>();
    }

    [Theory]
    [InlineData("   John   ", "John")]
    [InlineData(" Alice ", "Alice")]
    [InlineData(" Bob  ", "Bob")]
    public void Create_ShouldTrimLeadingAndTrailingWhitespace(string input, string expected)
    {
        // Arrange

        // Act
        var name = ProfileName.Create(input);

        // Assert
        name.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowProfileNameTooLongException()
    {
        // Arrange
        var longName = new string('A', ProfileName.MaxLength + 1);

        // Act
        var act = () => ProfileName.Create(longName);

        // Assert
        act.Should().Throw<ProfileNameTooLongException>();
    }
}