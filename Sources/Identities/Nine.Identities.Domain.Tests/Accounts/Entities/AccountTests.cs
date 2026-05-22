using FluentAssertions;

using Nine.Identities.Domain.Accounts.Enums;
using Nine.Identities.Domain.Accounts.Events;
using Nine.Identities.Domain.Accounts.Exceptions;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.Entities;

public sealed class AccountTests
{
    #region CreateInstance

    [Fact]
    public void CreateInstance_WithRequiredParameters_ShouldRaiseAndApplyAccountCreatedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().Build();

        // Act

        // Assert
        var accountCreatedDomainEventV1 = (AccountCreatedDomainEventV1)account.DomainEvents.Single();
        accountCreatedDomainEventV1.AccountId.Should().NotBeNull();
        accountCreatedDomainEventV1.EmailAddress.Value.Should().Be(AccountTestBuilder.DefaultEmailAddressValue);
        accountCreatedDomainEventV1.PhoneNumber?.Should().BeNull();
        accountCreatedDomainEventV1.InitialCredentialId.Should().NotBeNull();
        accountCreatedDomainEventV1.InitialCredentialType.Should().Be(AccountTestBuilder.DefaultCredentialType);
        accountCreatedDomainEventV1.InitialHashedSecret.Should().Be(AccountTestBuilder.DefaultHashedSecret);
        accountCreatedDomainEventV1.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.AccountId.Should().NotBeNull();
        account.EmailAddress.Value.Should().Be(AccountTestBuilder.DefaultEmailAddressValue);
        account.PhoneNumber.Should().BeNull();
        var credential = account.Credentials.Single();
        credential.Type.Should().Be(AccountTestBuilder.DefaultCredentialType);
        credential.Secret.Should().Be(AccountTestBuilder.DefaultHashedSecret);
    }

    [Fact]
    public void CreateInstance_WithAllParameters_ShouldRaiseAndApplyAccountCreatedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithOptionalParameters().Build();

        // Act

        // Assert
        var accountCreatedDomainEventV1 = (AccountCreatedDomainEventV1)account.DomainEvents.Single();
        accountCreatedDomainEventV1.EmailAddress.Value.Should().Be(AccountTestBuilder.DefaultEmailAddressValue);
        accountCreatedDomainEventV1.PhoneNumber?.Value.Should().Be(AccountTestBuilder.DefaultPhoneNumberValue);
        accountCreatedDomainEventV1.AccountId.Should().NotBeNull();
        accountCreatedDomainEventV1.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.EmailAddress.Value.Should().Be(AccountTestBuilder.DefaultEmailAddressValue);
        account.PhoneNumber?.Value.Should().Be(AccountTestBuilder.DefaultPhoneNumberValue);
        account.AccountId.Should().NotBeNull();
    }

    #endregion

    #region SetEmailAddress

    [Fact]
    public void SetEmailAddress_ShouldRaiseAndApplyAccountEmailAddressChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();
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
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();

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
        var account = new AccountTestBuilder().WithRequiredParameters().Build();
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
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();
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
            .WithRequiredParameters()
            .WithOptionalParameters()
            .WithoutCreationEvent()
            .Build();

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
            .WithRequiredParameters()
            .WithoutCreationEvent()
            .Build();

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
            .WithRequiredParameters()
            .WithPhoneNumber(AccountTestBuilder.DefaultPhoneNumberValue)
            .Build();
        account.VerifyPhoneNumber();
        account.ClearDomainEvents();

        // Act
        account.VerifyPhoneNumber();

        // Assert
        account.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region AddCredential

    [Fact]
    public void AddCredential_ShouldRaiseAndApplyCredentialAddedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();
        var newCredentialId = CredentialId.Create();
        var credentialType = CredentialType.OAuthGoogle;
        var secret = HashedSecret.Create("another");

        // Act
        account.AddCredential(
            credentialId: newCredentialId,
            type: credentialType,
            secret: secret
        );

        // Assert
        var credentialAddedDomainEventV1 = (CredentialAddedDomainEventV1)account.DomainEvents.Single();
        credentialAddedDomainEventV1.CredentialId.Should().Be(newCredentialId);
        credentialAddedDomainEventV1.CredentialType.Should().Be(credentialType);
        credentialAddedDomainEventV1.HashedSecret.Should().Be(secret);

        account.Credentials.Should().HaveCount(2);
        var credential = account.Credentials.Single(i => i.Id == newCredentialId);
        credential.Type.Should().Be(credentialType);
        credential.Secret.Should().Be(secret);
    }

    [Fact]
    public void AddCredential_WithDuplicateId_ShouldThrowCredentialAlreadyExistsException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().Build();
        var credential = account.Credentials.First();

        // Act
        var act = () => account.AddCredential(credential.Id, credential.Type, credential.Secret);

        // Assert
        act.Should().Throw<CredentialAlreadyExistsException>();
    }

    [Fact]
    public void AddCredential_WithDuplicateType_ShouldThrowCredentialAlreadyExistsException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().Build();
        var credential = account.Credentials.First();

        // Act
        var act = () => account.AddCredential(CredentialId.Create(), credential.Type, HashedSecret.Create("new-secret"));

        // Assert
        act.Should().Throw<CredentialAlreadyExistsException>();
    }

    #endregion

    #region RemoveCredential

    [Fact]
    public void RemoveCredential_ShouldRaiseAndApplyCredentialRemovedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();
        var extraId = CredentialId.Create();
        account.AddCredential(extraId, CredentialType.OAuthGoogle, HashedSecret.Create("extra"));
        account.ClearDomainEvents();

        // Act
        account.RemoveCredential(extraId);

        // Assert
        account.Credentials.Should().HaveCount(1);
        account.Credentials.Should().NotContain(c => c.Id == extraId);
        var credentialRemovedDomainEventV1 = (CredentialRemovedDomainEventV1)account.DomainEvents.Single();
        credentialRemovedDomainEventV1.CredentialId.Should().Be(extraId);
    }

    [Fact]
    public void RemoveCredential_WithUnknownId_ShouldThrowCredentialNotFoundException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();

        // Act
        var act = () => account.RemoveCredential(CredentialId.Create());

        // Assert
        act.Should().Throw<CredentialNotFoundException>();
    }

    [Fact]
    public void RemoveCredential_WhenLastCredential_ShouldThrowCannotRemoveLastCredentialException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();

        // Act
        var act = () => account.RemoveCredential(account.Credentials.Last().Id);

        // Assert
        act.Should().Throw<CannotRemoveLastCredentialException>();
    }

    #endregion

    #region ChangeCredential

    [Fact]
    public void ChangeCredential_ShouldRaiseAndApplyCredentialChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();
        var credentialId = account.Credentials.First().Id;
        var newSecret = HashedSecret.Create("new-secret");

        // Act
        account.ChangeCredential(credentialId, newSecret);

        // Assert
        var credentialChangedDomainEventV1 = (CredentialChangedDomainEventV1)account.DomainEvents.Single();
        credentialChangedDomainEventV1.CredentialId.Should().Be(credentialId);
        credentialChangedDomainEventV1.NewHashedSecret.Should().Be(newSecret);

        account.Credentials.Single().Secret.Should().Be(newSecret);
    }

    [Fact]
    public void ChangeCredential_WithUnknownId_ShouldThrowCredentialNotFoundException()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();

        // Act
        var act = () => account.ChangeCredential(CredentialId.Create(), HashedSecret.Create("new-secret"));

        // Assert
        act.Should().Throw<CredentialNotFoundException>();
    }

    #endregion
}