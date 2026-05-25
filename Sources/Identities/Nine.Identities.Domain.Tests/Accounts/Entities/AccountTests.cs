using FluentAssertions;

using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Contracts.Accounts.Enums;
using Nine.Identities.Domain.Contracts.Accounts.Events;
using Nine.Identities.Domain.Contracts.Accounts.Exceptions;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;
using Nine.Identities.Domain.Tests.Accounts.Builders;

namespace Nine.Identities.Domain.Tests.Accounts.Entities;

public sealed class AccountTests
{
    #region CreateWithPassword

    [Fact]
    public void CreateWithPassword_WithRequiredParameters_ShouldRaiseAndApplyAccountWithPasswordCreatedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder()
            .WithPasswordRequiredParameters()
            .BuildWithPassword();

        // Act

        // Assert
        var domainEvent = (AccountWithPasswordCreatedDomainEventV1)account.DomainEvents.Single();
        domainEvent.AccountId.Should().NotBeNull();
        domainEvent.EmailAddress.Should().Be(AccountTestBuilder.DefaultEmailAddress);
        domainEvent.PhoneNumber?.Should().BeNull();
        domainEvent.CredentialId.Should().NotBeNull();
        domainEvent.HashedPassword.Should().Be(AccountTestBuilder.DefaultHashedPassword);
        domainEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.AccountId.Should().NotBeNull();
        account.EmailAddress.Should().Be(AccountTestBuilder.DefaultEmailAddress);
        account.PhoneNumber.Should().BeNull();
        var credential = (PasswordCredential)account.Credentials.Single();
        credential.Type.Should().Be(CredentialType.Password);
        credential.HashedPassword.Should().Be(AccountTestBuilder.DefaultHashedPassword);
    }

    [Fact]
    public void CreateWithPassword_WithAllParameters_ShouldRaiseAndApplyAccountWithPasswordCreatedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder()
            .WithPasswordRequiredParameters()
            .WithPasswordOptionalParameters()
            .BuildWithPassword();

        // Act

        // Assert
        var domainEvent = (AccountWithPasswordCreatedDomainEventV1)account.DomainEvents.Single();
        domainEvent.AccountId.Should().NotBeNull();
        domainEvent.EmailAddress.Should().Be(AccountTestBuilder.DefaultEmailAddress);
        domainEvent.PhoneNumber.Should().Be(AccountTestBuilder.DefaultPhoneNumber);
        domainEvent.CredentialId.Should().NotBeNull();
        domainEvent.HashedPassword.Should().Be(AccountTestBuilder.DefaultHashedPassword);
        domainEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.AccountId.Should().NotBeNull();
        account.EmailAddress.Should().Be(AccountTestBuilder.DefaultEmailAddress);
        account.PhoneNumber.Should().Be(AccountTestBuilder.DefaultPhoneNumber);
        var credential = (PasswordCredential)account.Credentials.Single();
        credential.Type.Should().Be(CredentialType.Password);
        credential.HashedPassword.Should().Be(AccountTestBuilder.DefaultHashedPassword);
    }

    #endregion

    #region SetEmailAddress

    [Fact]
    public void SetEmailAddress_ShouldRaiseAndApplyAccountEmailAddressChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().WithoutCreationEvent().BuildWithPassword();
        var newEmailAddress = EmailAddress.Create("jane@example.com");

        // Act
        account.SetEmailAddress(newEmailAddress);

        // Assert
        var accountEmailAddressChangedDomainEventV1 = (AccountEmailAddressChangedDomainEventV1)account.DomainEvents.Single();
        accountEmailAddressChangedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountEmailAddressChangedDomainEventV1.EmailAddress.Should().Be(newEmailAddress);
        accountEmailAddressChangedDomainEventV1.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.EmailAddress.Should().Be(newEmailAddress);
        account.IsEmailAddressVerified.Should().Be(false);
    }

    #endregion

    #region VerifyEmailAddress

    [Fact]
    public void VerifyEmailAddress_ShouldRaiseAndApplyAccountEmailAddressVerifiedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().WithoutCreationEvent().BuildWithPassword();

        // Act
        account.VerifyEmailAddress();

        // Assert
        var accountEmailAddressVerifiedDomainEventV1 = (AccountEmailAddressVerifiedDomainEventV1)account.DomainEvents.Single();
        accountEmailAddressVerifiedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountEmailAddressVerifiedDomainEventV1.EmailAddress.Should().Be(account.EmailAddress);
        accountEmailAddressVerifiedDomainEventV1.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.IsEmailAddressVerified.Should().Be(true);
    }

    [Fact]
    public void VerifyEmailAddress_ShouldNotRaiseAccountEmailAddressVerifiedDomainEventV1_WhenEmailAddressIsAlreadyVerified()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().BuildWithPassword();
        account.VerifyEmailAddress();
        account.ClearDomainEvents();

        // Act
        account.VerifyEmailAddress();

        // Assert
        account.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region SetPhoneNumber

    [Fact]
    public void SetPhoneNumber_ShouldRaiseAndApplyAccountPhoneNumberChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().WithoutCreationEvent().BuildWithPassword();
        var newPhoneNumber = PhoneNumber.Create("+987654321");

        // Act
        account.SetPhoneNumber(newPhoneNumber);

        // Assert
        var accountPhoneNumberChangedDomainEventV1 =
            (AccountPhoneNumberChangedDomainEventV1)account.DomainEvents.Single();
        accountPhoneNumberChangedDomainEventV1.PhoneNumber.Should().Be(newPhoneNumber);
        accountPhoneNumberChangedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountPhoneNumberChangedDomainEventV1.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.PhoneNumber.Should().Be(newPhoneNumber);
        account.IsPhoneNumberVerified.Should().Be(false);
    }

    #endregion

    #region VerifyPhoneNumber

    [Fact]
    public void VerifyPhoneNumber_ShouldRaiseAndApplyAccountPhoneNumberVerifiedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder()
            .WithPasswordRequiredParameters()
            .WithPasswordOptionalParameters()
            .WithoutCreationEvent()
            .BuildWithPassword();

        // Act
        account.VerifyPhoneNumber();

        // Assert
        var accountPhoneNumberVerifiedDomainEventV1 = (AccountPhoneNumberVerifiedDomainEventV1)account.DomainEvents.Single();
        accountPhoneNumberVerifiedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountPhoneNumberVerifiedDomainEventV1.PhoneNumber.Should().Be(account.PhoneNumber);
        accountPhoneNumberVerifiedDomainEventV1.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.IsPhoneNumberVerified.Should().Be(true);
    }

    [Fact]
    public void VerifyPhoneNumber_WhenPhoneNumberNotSet_ShouldThrowAccountPhoneNumberNotSetException()
    {
        // Arrange
        var account = new AccountTestBuilder()
            .WithPasswordRequiredParameters()
            .WithoutCreationEvent()
            .BuildWithPassword();

        // Act
        var act = () => account.VerifyPhoneNumber();

        // Assert
        act.Should().Throw<AccountPhoneNumberNotSetException>();
    }

    [Fact]
    public void VerifyPhoneNumber_ShouldNotRaiseAccountPhoneNumberVerifiedDomainEventV1_WhenPhoneNumberIsAlreadyVerified()
    {
        // Arrange
        var account = new AccountTestBuilder()
            .WithPasswordRequiredParameters()
            .WithPhoneNumber(AccountTestBuilder.DefaultPhoneNumber)
            .BuildWithPassword();
        account.VerifyPhoneNumber();
        account.ClearDomainEvents();

        // Act
        account.VerifyPhoneNumber();

        // Assert
        account.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region AddPasswordCredential

    [Fact]
    public void AddPasswordCredential_ShouldRaiseAndApplyCredentialAddedDomainEventV1()
    {
        throw new NotImplementedException();

        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void AddPasswordCredential_WithExistingPasswordCredential_ShouldThrowCredentialAlreadyExistsException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().BuildWithPassword();

        // Act
        var act = () => account.AddPasswordCredential(HashedSecret.Create("new-secret"));

        // Assert
        act.Should().Throw<CredentialAlreadyExistsException>();
    }

    #endregion
    
    #region ChangePasswordCredential

    [Fact]
    public void ChangePasswordCredential_ShouldRaiseAndApplyCredentialChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder()
            .WithPasswordRequiredParameters()
            .WithoutCreationEvent()
            .BuildWithPassword();
        var credentialId = account.Credentials.First().Id;
        var newHashedPassword = HashedSecret.Create("new-secret");

        // Act
        account.ChangePasswordCredential(credentialId, newHashedPassword);

        // Assert
        var domainEvent = (AccountPasswordCredentialChangedDomainEventV1)account.DomainEvents.Single();
        domainEvent.CredentialId.Should().Be(credentialId);
        domainEvent.NewHashedPassword.Should().Be(newHashedPassword);

        var credential = (PasswordCredential)account.Credentials.Single();
        credential.HashedPassword.Should().Be(newHashedPassword);
    }

    [Fact]
    public void ChangePasswordCredential_WithUnknownId_ShouldThrowCredentialNotFoundException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().WithoutCreationEvent().BuildWithPassword();

        // Act
        var act = () => account.ChangePasswordCredential(CredentialId.Create(), HashedSecret.Create("new-secret"));

        // Assert
        act.Should().Throw<CredentialNotFoundException>();
    }

    #endregion

    #region RemoveCredential

    [Fact]
    public void RemoveCredential_ShouldRaiseAndApplyCredentialRemovedDomainEventV1()
    {
        throw new NotImplementedException();

        // Arrange

        // Act

        // Assert
    }

    [Fact]
    public void RemoveCredential_WithUnknownId_ShouldThrowCredentialNotFoundException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().WithoutCreationEvent().BuildWithPassword();

        // Act
        var act = () => account.RemoveCredential(CredentialId.Create());

        // Assert
        act.Should().Throw<CredentialNotFoundException>();
    }

    [Fact]
    public void RemoveCredential_WhenLastCredential_ShouldThrowCannotRemoveLastCredentialException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithPasswordRequiredParameters().WithoutCreationEvent().BuildWithPassword();

        // Act
        var act = () => account.RemoveCredential(account.Credentials.Last().Id);

        // Assert
        act.Should().Throw<CannotRemoveLastCredentialException>();
    }

    #endregion
}
