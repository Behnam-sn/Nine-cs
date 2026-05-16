using FluentAssertions;

using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.ValueObjects;

public sealed class UserIdTests
{
    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Arrange

        // Act
        var id1 = UserId.Create();
        var id2 = UserId.Create();

        // Assert
        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }
    
    [Fact]
    public void Parse_ShouldReturnCorrectId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var userId = UserId.Parse(guid);

        // Assert
        userId.Value.Should().Be(guid);
        userId.ToString().Should().Be(guid.ToString());
    }
}