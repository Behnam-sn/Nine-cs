using FluentAssertions;

using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.ValueObjects;

public sealed class CredentialIdTests
{
    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Arrange

        // Act
        var id1 = CredentialId.Create();
        var id2 = CredentialId.Create();

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
        var credentialId = CredentialId.From(guid);

        // Assert
        credentialId.Value.Should().Be(guid);
    }
}