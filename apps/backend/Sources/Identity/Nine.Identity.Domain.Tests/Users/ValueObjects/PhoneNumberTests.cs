using FluentAssertions;

using Nine.Identity.Domain.Contracts.Users.Exceptions;
using Nine.Identity.Domain.Contracts.Users.ValueObjects;

namespace Nine.Identity.Domain.Tests.Users.ValueObjects;

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
    [InlineData("5551234567")]
    [InlineData("abc123")]
    [InlineData("+")]
    [InlineData("+123")]
    [InlineData("+12345678901234567")]
    public void Create_WithInvalidFormat_ShouldThrowPhoneNumberInvalidFormatException(string invalidPhoneNumber)
    {
        // Arrange

        // Act
        var act = () => PhoneNumber.Create(invalidPhoneNumber);

        // Assert
        act.Should().Throw<PhoneNumberInvalidFormatException>();
    }
}
