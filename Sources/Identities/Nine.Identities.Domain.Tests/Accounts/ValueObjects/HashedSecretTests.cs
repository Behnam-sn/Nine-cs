using FluentAssertions;

using Nine.Identities.Domain.Accounts.Exceptions;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.ValueObjects;

public sealed class HashedSecretTests
{
    [Fact]
    public void Create_ShouldSetValue()
    {
        // Arrange
        const string secret = "my-secret-hash";

        // Act
        var hashedSecret = HashedSecret.Create(secret);

        // Assert
        hashedSecret.Value.Should().Be(secret);
    }

    [Theory]
    [InlineData("  some-hash  ", "some-hash")]
    [InlineData("  another  ", "another")]
    public void Create_ShouldTrimLeadingAndTrailingWhitespace(string input, string expected)
    {
        // Arrange

        // Act
        var hashedSecret = HashedSecret.Create(input);

        // Assert
        hashedSecret.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowHashedSecretCannotBeEmptyException(string? invalidInput)
    {
        // Arrange

        // Act
        var act = () => HashedSecret.Create(invalidInput);

        // Assert
        act.Should().Throw<HashedSecretCannotBeEmptyException>();
    }

    [Fact]
    public void Equality_ShouldBeBasedOnValue()
    {
        // Arrange
        var secret1 = HashedSecret.Create("hash");
        var secret2 = HashedSecret.Create("hash");
        var secret3 = HashedSecret.Create("different");

        // Act

        // Assert
        secret1.Should().Be(secret2);
        secret1.GetHashCode().Should().Be(secret2.GetHashCode());
        secret1.Should().NotBe(secret3);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var secret = HashedSecret.Create("hash123");

        // Act

        // Assert
        secret.ToString().Should().Be("hash123");
    }
}
