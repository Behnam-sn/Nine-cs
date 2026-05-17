using FluentAssertions;

using Nine.Identities.Domain.Users.Events;
using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.Entities;

public sealed class UserTests
{
    [Fact]
    public void CreateInstance_ShouldRaiseAndApplyUserCreatedDomainEventV1()
    {
        // Arrange
        var user = new UserTestBuilder().Build();

        // Act

        // Assert
        var userCreatedDomainEventV1 = (UserCreatedDomainEventV1)user.DomainEvents.Single();
        userCreatedDomainEventV1.Name.Value.Should().Be(UserTestBuilder.DefaultNameValue);
        userCreatedDomainEventV1.Email.Value.Should().Be(UserTestBuilder.DefaultEmailValue);
        userCreatedDomainEventV1.PhoneNumber.Value.Should().Be(UserTestBuilder.DefaultPhoneValue);
        userCreatedDomainEventV1.UserId.Should().NotBeNull();
        userCreatedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        user.Name.Value.Should().Be(UserTestBuilder.DefaultNameValue);
        user.Email.Value.Should().Be(UserTestBuilder.DefaultEmailValue);
        user.PhoneNumber.Value.Should().Be(UserTestBuilder.DefaultPhoneValue);
        user.UserId.Should().NotBeNull();
    }

    [Fact]
    public void SetName_ShouldRaiseAndApplyUserNameChangedDomainEventV1()
    {
        // Arrange
        var user = new UserTestBuilder().WithoutCreationEvent().Build();
        var newName = Name.Create("Jane Doe");

        // Act
        user.SetName(newName);

        // Assert
        var userNameChangedDomainEventV1 = (UserNameChangedDomainEventV1)user.DomainEvents.Single();
        userNameChangedDomainEventV1.UserId.Should().Be(user.UserId);
        userNameChangedDomainEventV1.Name.Should().Be(newName);
        userNameChangedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        user.Name.Should().Be(newName);
    }

    [Fact]
    public void SetEmail_ShouldRaiseAndApplyUserEmailChangedDomainEventV1()
    {
        // Arrange
        var user = new UserTestBuilder().WithoutCreationEvent().Build();
        var newEmail = Email.Create("jane@example.com");

        // Act
        user.SetEmail(newEmail);

        // Assert
        var userEmailChangedDomainEventV1 = (UserEmailChangedDomainEventV1)user.DomainEvents.Single();
        userEmailChangedDomainEventV1.UserId.Should().Be(user.UserId);
        userEmailChangedDomainEventV1.Email.Should().Be(newEmail);
        userEmailChangedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        user.Email.Should().Be(newEmail);
    }

    [Fact]
    public void SetPhoneNumber_ShouldRaiseAndApplyUserPhoneNumberChangedDomainEventV1()
    {
        // Arrange
        var user = new UserTestBuilder().WithoutCreationEvent().Build();
        var newPhoneNumber = PhoneNumber.Create("+987654321");

        // Act
        user.SetPhoneNumber(newPhoneNumber);

        // Assert
        var userPhoneNumberChangedDomainEventV1 = (UserPhoneNumberChangedDomainEventV1)user.DomainEvents.Single();
        userPhoneNumberChangedDomainEventV1.PhoneNumber.Should().Be(newPhoneNumber);
        userPhoneNumberChangedDomainEventV1.UserId.Should().Be(user.UserId);
        userPhoneNumberChangedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        user.PhoneNumber.Should().Be(newPhoneNumber);
    }
}