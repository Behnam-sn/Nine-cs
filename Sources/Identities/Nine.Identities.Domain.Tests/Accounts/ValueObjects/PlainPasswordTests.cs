using FluentAssertions;

using Nine.Identities.Domain.Contracts.Accounts.Exceptions;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.ValueObjects;

public sealed class PlainPasswordTests
{
    [Theory]
    [InlineData("Aa1!bcde")]
    [InlineData("Z9$abcdef")]
    [InlineData("P@ssw0rd")]
    [InlineData("Str0ng!Pass")]
    public void Create_ShouldSetValue(string validPassword)
    {
        // Arrange

        // Act
        var plainPassword = PlainPassword.Create(validPassword);

        // Assert
        plainPassword.Value.Should().Be(validPassword);
    }
    
    [Fact]
    public void Create_Null_ThrowsPasswordEmptyException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create(null);
        
        // Assert
        act.Should().Throw<PasswordEmptyException>();
    }

    [Fact]
    public void Create_EmptyString_ThrowsPasswordEmptyException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create(string.Empty);
        
        // Assert
        act.Should().Throw<PasswordEmptyException>();
    }
    
    [Fact]
    public void Create_ShorterThanMinimum_ThrowsPasswordTooShortException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create("Abc1!");
        
        // Assert
        act.Should().Throw<PasswordTooShortException>()
            .Which.MinimumLength.Should().Be(PlainPassword.MinimumLength);
    }

    [Fact]
    public void Create_Whitespace_ThrowsPasswordHavingWhiteSpaceException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create("abcdef1! ");
        
        // Assert
        act.Should().Throw<PasswordHavingWhiteSpaceException>();
    }
    
    [Fact]
    public void Create_NoUppercase_ThrowsPasswordMissingUppercaseException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create("abcdef1!");
        
        // Assert
        act.Should().Throw<PasswordMissingUppercaseException>();
    }

    [Fact]
    public void Create_NoLowercase_ThrowsPasswordMissingLowercaseException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create("ABCDEF1!");
        
        // Assert
        act.Should().Throw<PasswordMissingLowercaseException>();
    }

    [Fact]
    public void Create_NoDigit_ThrowsPasswordMissingDigitException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create("Abcdefgh!");
        
        // Assert
        act.Should().Throw<PasswordMissingDigitException>();
    }

    [Fact]
    public void Create_NoSpecialCharacter_ThrowsPasswordMissingSpecialCharacterException()
    {
        // Arrange

        // Act
        var act = () => PlainPassword.Create("Abcdef1g");
        
        // Assert
        act.Should().Throw<PasswordMissingSpecialCharacterException>();
    }
}
