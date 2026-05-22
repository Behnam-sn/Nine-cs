using FluentAssertions;

using Nine.Identities.Domain.Contracts.Accounts.Exceptions;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.ValueObjects;

public sealed class NameTests
{
    [Theory]
    [InlineData("John Doe")]
    [InlineData("Alice")]
    [InlineData("A")]
    public void Create_ShouldSetValue(string value)
    {
        // Arrange

        // Act
        var name = Name.Create(value);

        // Assert
        name.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyOrWhitespace_ShouldThrowNameCannotBeEmptyException(string? invalidValue)
    {
        // Arrange

        // Act
        var act = () => Name.Create(invalidValue);

        // Assert
        act.Should().Throw<NameCannotBeEmptyException>();
    }

    [Theory]
    [InlineData("   John   ", "John")]
    [InlineData(" Alice ", "Alice")]
    [InlineData(" Bob  ", "Bob")]
    public void Create_ShouldTrimLeadingAndTrailingWhitespace(string input, string expected)
    {
        var name = Name.Create(input);

        name.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowNameTooLongException()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var act = () => Name.Create(longName);

        // Assert
        act.Should().Throw<NameTooLongException>();
    }
}
