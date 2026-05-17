using FluentAssertions;

using Nine.Identities.Domain.Users.Exceptions;
using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.ValueObjects;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+1 555-123-4567")]
    [InlineData("5551234567")]
    [InlineData("(555) 123-4567")]
    [InlineData("11223344")]
    [InlineData("11 22 33 44")]
    [InlineData("01211223344")]
    [InlineData("012 11 22 33 44")]
    [InlineData("+98 11 22 33 44")]
    [InlineData("01234567890")]
    [InlineData("0123 456 7890")]
    [InlineData("+98 123 456 7890")]
    public void Create_ShouldSetValue(string input)
    {
        // Arrange

        // Act
        var phoneNumber = PhoneNumber.Create(input);

        // Assert
        phoneNumber.Value.Should().Be(input);
    }

    [Theory]
    [InlineData(" 5551234567 ", "5551234567")]
    [InlineData(" +1 555-123-4567 ", "+1 555-123-4567")]
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
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowPhoneNumberCannotBeEmptyException(string invalidInput)
    {
        // Arrange

        // Act
        var act = () => PhoneNumber.Create(invalidInput);

        // Assert
        act.Should().Throw<PhoneNumberCannotBeEmptyException>();
    }
    
    [Theory]
    [InlineData("abc123")]
    [InlineData("123!@#")]
    [InlineData("++1234567")]
    public void Create_WithInvalidFormat_ShouldThrowPhoneNumberInvalidFormatException(string invalidPhone)
    {
        // Arrange

        // Act
        var act = () => PhoneNumber.Create(invalidPhone);

        // Assert
        act.Should().Throw<PhoneNumberInvalidFormatException>();
    }
}