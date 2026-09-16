using FluentAssertions;

using Nine.Identities.Domain.Contracts.Users.Exceptions;
using Nine.Identities.Domain.Contracts.Users.ValueObjects;

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
    public void From_ShouldReturnCorrectId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = UserId.From(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void From_WithEmptyGuid_ShouldThrowUserIdCannotBeEmptyException()
    {
        // Arrange

        // Act
        var act = () => UserId.From(Guid.Empty);

        // Assert
        act.Should().Throw<UserIdCannotBeEmptyException>();
    }

    [Fact]
    public void Parse_ShouldReturnCorrectId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = UserId.Parse(guid.ToString());

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void Parse_WithInvalidFormat_ShouldThrowUserIdInvalidFormatException()
    {
        // Arrange

        // Act
        var act = () => UserId.Parse("not-a-guid");

        // Assert
        act.Should().Throw<UserIdInvalidFormatException>();
    }
}
