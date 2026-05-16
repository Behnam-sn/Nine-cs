using FluentAssertions;

using Nine.Identities.Domain.Users.Exceptions;
using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.ValueObjects;

public sealed class NameTests
{
    [Theory]
    [InlineData("John Doe")]
    [InlineData("Alice")]
    [InlineData("A")]
    public void Create_ShouldSetValue(string value)
    {
        // Arrange

        // Act
        var name = Name.Create(value);

        // Assert
        name.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowNameCannotBeEmptyException(string invalidValue)
    {
        // Arrange

        // Act
        var act = () => Name.Create(invalidValue);

        // Assert
        act.Should().Throw<NameCannotBeEmptyException>();
    }
    
    [Fact]
    public void Create_WithTooLongValue_ShouldThrowNameTooLongException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => Name.Create(longName);

        // Assert
        act.Should().Throw<NameTooLongException>();
    }
}