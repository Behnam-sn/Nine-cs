using FluentAssertions;

using Nine.Identities.Domain.Accounts.Exceptions;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.ValueObjects;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("john@example.com")]
    [InlineData("alice@domain.co")]
    [InlineData("a@b.io")]
    public void Create_ShouldSetValue(string input)
    {
        // Arrange

        // Act
        var emailAddress = EmailAddress.Create(input);

        // Assert
        emailAddress.Value.Should().Be(input);
    }

    [Theory]
    [InlineData("  John@Example.com  ", "john@example.com")]
    [InlineData(" ALICE@DOMAIN.CO ", "alice@domain.co")]
    [InlineData("a@b.io", "a@b.io")]
    public void Create_ShouldNormaliseValue(string input, string expected)
    {
        // Arrange

        // Act
        var emailAddress = EmailAddress.Create(input);

        // Assert
        emailAddress.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowEmailAddressCannotBeEmptyException(string? input)
    {
        // Arrange

        // Act
        var act = () => EmailAddress.Create(input);

        // Assert
        act.Should().Throw<EmailAddressCannotBeEmptyException>();
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("missing@domain")]
    [InlineData("missing.domain.com")]
    [InlineData("@nouser.com")]
    public void Create_WithInvalidFormat_ShouldThrowEmailAddressInvalidFormatException(string input)
    {
        // Arrange

        // Act
        var act = () => EmailAddress.Create(input);

        // Assert
        act.Should().Throw<EmailAddressInvalidFormatException>();
    }
}
