using FluentAssertions;

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
}