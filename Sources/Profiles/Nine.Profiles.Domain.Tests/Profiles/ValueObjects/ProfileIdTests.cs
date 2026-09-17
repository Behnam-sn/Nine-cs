using FluentAssertions;
using Nine.Profiles.Domain.Contracts.Profiles.Exceptions;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;

namespace Nine.Profiles.Domain.Tests.Profiles.ValueObjects;

public sealed class ProfileIdTests
{
    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Arrange

        // Act
        var id1 = ProfileId.Create();
        var id2 = ProfileId.Create();

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
        var accountId = ProfileId.From(guid);

        // Assert
        accountId.Value.Should().Be(guid);
    }

    [Fact]
    public void From_WithEmptyGuid_ShouldThrowProfileIdCannotBeEmptyException()
    {
        // Arrange

        // Act
        var act = () => ProfileId.From(Guid.Empty);

        // Assert
        act.Should().Throw<ProfileIdCannotBeEmptyException>();
    }

    [Fact]
    public void Parse_ShouldReturnCorrectId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var accountId = ProfileId.Parse(guid.ToString());

        // Assert
        accountId.Value.Should().Be(guid);
    }

    [Fact]
    public void Parse_WithInvalidFormat_ShouldThrowProfileIdInvalidFormatException()
    {
        // Arrange

        // Act
        var act = () => ProfileId.Parse("not-a-guid");

        // Assert
        act.Should().Throw<ProfileIdInvalidFormatException>();
    }
}