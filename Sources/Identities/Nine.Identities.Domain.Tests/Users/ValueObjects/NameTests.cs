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
    
    [Fact]
    public void Create_WithNull_ShouldThrowNameCannotBeEmptyException()
    {
        // Arrange

        // Act
        var act = () => Name.Create(null);

        // Assert
        act.Should().Throw<NameCannotBeEmptyException>();
    }
}