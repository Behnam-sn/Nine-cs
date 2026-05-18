using FluentAssertions;

using Nine.Identities.Domain.Accounts.Events;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.Entities;

public sealed class AccountTests
{
    [Fact]
    public void CreateInstance_ShouldRaiseAndApplyAccountCreatedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().Build();

        // Act

        // Assert
        var accountCreatedDomainEventV1 = (AccountCreatedDomainEventV1)account.DomainEvents.Single();
        accountCreatedDomainEventV1.Email.Value.Should().Be(AccountTestBuilder.DefaultEmailValue);
        accountCreatedDomainEventV1.PhoneNumber.Value.Should().Be(AccountTestBuilder.DefaultPhoneValue);
        accountCreatedDomainEventV1.AccountId.Should().NotBeNull();
        accountCreatedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        account.Email.Value.Should().Be(AccountTestBuilder.DefaultEmailValue);
        account.PhoneNumber.Value.Should().Be(AccountTestBuilder.DefaultPhoneValue);
        account.AccountId.Should().NotBeNull();
    }

    [Fact]
    public void SetEmail_ShouldRaiseAndApplyAccountEmailChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithoutCreationEvent().Build();
        var newEmail = Email.Create("jane@example.com");

        // Act
        account.SetEmail(newEmail);

        // Assert
        var accountEmailChangedDomainEventV1 = (AccountEmailChangedDomainEventV1)account.DomainEvents.Single();
        accountEmailChangedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountEmailChangedDomainEventV1.Email.Should().Be(newEmail);
        accountEmailChangedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        account.Email.Should().Be(newEmail);
    }

    [Fact]
    public void SetPhoneNumber_ShouldRaiseAndApplyAccountPhoneNumberChangedDomainEventV1()
    {
        // Arrange
        var account = new AccountTestBuilder().WithoutCreationEvent().Build();
        var newPhoneNumber = PhoneNumber.Create("+987654321");

        // Act
        account.SetPhoneNumber(newPhoneNumber);

        // Assert
        var accountPhoneNumberChangedDomainEventV1 = (AccountPhoneNumberChangedDomainEventV1)account.DomainEvents.Single();
        accountPhoneNumberChangedDomainEventV1.PhoneNumber.Should().Be(newPhoneNumber);
        accountPhoneNumberChangedDomainEventV1.AccountId.Should().Be(account.AccountId);
        accountPhoneNumberChangedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        account.PhoneNumber.Should().Be(newPhoneNumber);
    }
}