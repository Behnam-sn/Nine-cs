using FluentAssertions;

using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.ValueObjects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("john@example.com")]
    [InlineData("alice@domain.co")]
    [InlineData("a@b.io")]
    public void Create_ShouldSetValue(string email)
    {
        // Arrange

        // Act
        var e = Email.Create(email);

        // Assert
        e.Value.Should().Be(email);
    }
}