using FluentAssertions;
using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Domain.Tests.Profiles.ValueObjects;

public sealed class ProfileHandleTests
{
    [Theory]
    [InlineData("johndoe")]
    [InlineData("john_doe")]
    [InlineData("alice")]
    [InlineData("abc")]
    public void Create_ShouldSetValue(string value)
    {
        // Arrange

        // Act
        var name = ProfileHandle.Create(value);

        // Assert
        name.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("JohnDoe", "johndoe")]
    public void Create_ShouldFormatValueToLowerCase(string input, string expected)
    {
        // Arrange

        // Act
        var name = ProfileHandle.Create(input);

        // Assert
        name.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithTooShortValue_ShouldThrowProfileHandleTooShortException()
    {
        // Arrange
        var value = new string('A', ProfileHandle.MinLength - 1);

        // Act
        var act = () => ProfileHandle.Create(value);

        // Assert
        act.Should().Throw<ProfileHandleTooShortException>();
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowProfileHandleTooLongException()
    {
        // Arrange
        var value = new string('A', ProfileHandle.MaxLength + 1);

        // Act
        var act = () => ProfileHandle.Create(value);

        // Assert
        act.Should().Throw<ProfileHandleTooLongException>();
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("john doe")]
    [InlineData("john.doe")]
    [InlineData("john-doe")]
    [InlineData("john!doe")]
    [InlineData("john?doe")]
    [InlineData("john@doe")]
    public void Create_WithInvalidCharacters_ShouldThrowProfileHandleInvalidCharactersException(string invalidValue)
    {
        // Arrange

        // Act
        var act = () => ProfileHandle.Create(invalidValue);

        // Assert
        act.Should().Throw<ProfileHandleInvalidCharactersException>();
    }
}