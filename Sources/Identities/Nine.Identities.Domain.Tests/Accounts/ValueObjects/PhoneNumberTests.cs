using FluentAssertions;

using Nine.Identities.Domain.Contracts.Accounts.Exceptions;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.ValueObjects;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+1 555-123-4567", "+15551234567")]
    [InlineData("+98 11 22 33 44", "+9811223344")]
    [InlineData("+44 20 7946 0958", "+442079460958")]
    public void Create_ShouldNormaliseToE164(string input, string expected)
    {
        // Arrange

        // Act
        var phoneNumber = PhoneNumber.Create(input);

        // Assert
        phoneNumber.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("  +1 555-123-4567  ", "+15551234567")]
    public void Create_ShouldTrimLeadingAndTrailingWhitespace(string input, string expected)
    {
        // Arrange

        // Act
        var phoneNumber = PhoneNumber.Create(input);

        // Assert
        phoneNumber.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowPhoneNumberCannotBeEmptyException(string? invalidInput)
    {
        // Arrange

        // Act
        var act = () => PhoneNumber.Create(invalidInput);

        // Assert
        act.Should().Throw<PhoneNumberCannotBeEmptyException>();
    }

    [Theory]
    [InlineData("5551234567")]            // no leading +
    [InlineData("abc123")]                // letters
    [InlineData("+")]                     // just plus
    [InlineData("+123")]                  // too short (< 7 digits after country code)
    [InlineData("+12345678901234567")]    // too long (>15 digits total)
    public void Create_WithInvalidFormat_ShouldThrowPhoneNumberInvalidFormatException(string invalidPhoneNumber)
    {
        // Arrange

        // Act
        var act = () => PhoneNumber.Create(invalidPhoneNumber);

        // Assert
        act.Should().Throw<PhoneNumberInvalidFormatException>();
    }
}
