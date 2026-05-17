using FluentAssertions;

using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.ValueObjects;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+1 555-123-4567")]
    [InlineData("5551234567")]
    [InlineData("(555) 123-4567")]
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
}