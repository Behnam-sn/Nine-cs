using FluentAssertions;

using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.ValueObjects;

public sealed class AccountIdTests
{
    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        // Arrange

        // Act
        var id1 = AccountId.Create();
        var id2 = AccountId.Create();

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
        var accountId = AccountId.From(guid);

        // Assert
        accountId.Value.Should().Be(guid);
    }
    
    [Fact]
    public void Parse_ShouldReturnCorrectId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var accountId = AccountId.Parse(guid);

        // Assert
        accountId.Value.Should().Be(guid);
        accountId.ToString().Should().Be(guid.ToString());
    }
}