using FluentAssertions;

using Nine.Identities.Domain.Users.Exceptions;
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

    [Theory]
    [InlineData("  john@example.com  ", "john@example.com")]
    [InlineData(" alice@domain.co ", "alice@domain.co")]
    public void Create_ShouldTrimLeadingAndTrailingWhitespace(string input, string expected)
    {
        // Arrange

        // Act
        var e = Email.Create(input);

        // Assert
        e.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowEmailCannotBeEmptyException(string invalidEmail)
    {
        // Arrange

        // Act
        var act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<EmailCannotBeEmptyException>();
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("missing@domain")]
    [InlineData("missing.domain.com")]
    [InlineData("@nouser.com")]
    public void Create_WithInvalidFormat_ShouldThrowEmailInvalidFormatException(string invalidEmail)
    {
        // Arrange

        // Act
        var act = () => Email.Create(invalidEmail);

        // Assert
        act.Should().Throw<EmailInvalidFormatException>();
    }
}