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
        accountCreatedDomainEventV1.Email.Value.Should().Be(AccountTestBuilder.DefaultEmailValue);
        accountCreatedDomainEventV1.PhoneNumber?.Should().BeNull();
        accountCreatedDomainEventV1.InitialCredentialId.Should().NotBeNull();
        accountCreatedDomainEventV1.InitialCredentialType.Should().Be(AccountTestBuilder.DefaultCredentialType);
        accountCreatedDomainEventV1.InitialHashedSecret.Should().Be(AccountTestBuilder.DefaultHashedSecret);
        accountCreatedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.AccountId.Should().NotBeNull();
        account.Email.Value.Should().Be(AccountTestBuilder.DefaultEmailValue);
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
        accountCreatedDomainEventV1.Email.Value.Should().Be(AccountTestBuilder.DefaultEmailValue);
        accountCreatedDomainEventV1.PhoneNumber?.Value.Should().Be(AccountTestBuilder.DefaultPhoneValue);
        accountCreatedDomainEventV1.AccountId.Should().NotBeNull();
        accountCreatedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.Email.Value.Should().Be(AccountTestBuilder.DefaultEmailValue);
        account.PhoneNumber?.Value.Should().Be(AccountTestBuilder.DefaultPhoneValue);
        account.AccountId.Should().NotBeNull();
    }

    #endregion

    #region SetEmail

    [Fact]
    public void SetEmail_ShouldRaiseAndApplyAccountEmailChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();
        var newEmail = Email.Create("jane@example.com");

        // Act
        account.SetEmail(newEmail);

        // Assert
        var accountEmailChangedDomainEventV1 = (AccountEmailChangedDomainEventV1)account.DomainEvents.Single();
        accountEmailChangedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountEmailChangedDomainEventV1.Email.Should().Be(newEmail);
        accountEmailChangedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.Email.Should().Be(newEmail);
        account.IsEmailVerified.Should().Be(false);
    }

    #endregion

    #region VerifyEmail

    [Fact]
    public void VerifyEmail_ShouldRaiseAndApplyAccountEmailVerifiedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();

        // Act
        account.VerifyEmail();

        // Assert
        var accountEmailVerifiedDomainEventV1 = (AccountEmailVerifiedDomainEventV1)account.DomainEvents.Single();
        accountEmailVerifiedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountEmailVerifiedDomainEventV1.Email.Should().Be(account.Email);
        accountEmailVerifiedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.IsEmailVerified.Should().Be(true);
    }

    [Fact]
    public void VerifyEmail_ShouldNotRaiseAccountEmailVerifiedDomainEventV1_WhenEmailIsAlreadyVerified()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().Build();
        account.VerifyEmail();
        account.ClearDomainEvents();

        // Act
        account.VerifyEmail();

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
        accountPhoneNumberChangedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

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
        accountPhoneNumberVerifiedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        account.IsPhoneNumberVerified.Should().Be(true);
    }

    [Fact]
    public void VerifyPhoneNumber_WhenPhoneNotSet_ShouldThrowAccountPhoneNumberNotSetException()
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
            .WithPhoneNumber(AccountTestBuilder.DefaultPhoneValue)
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

    #endregion

    #region RemoveCredential

    [Fact]
    public void RemoveCredential_ShouldRaiseAndApplyCredentialRemovedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithRequiredParameters().WithoutCreationEvent().Build();
        var extraId = CredentialId.Create();
        account.AddCredential(extraId, CredentialType.Password, HashedSecret.Create("extra"));
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