using FluentAssertions;

using Nine.Identities.Domain.Users.Entities;
using Nine.Identities.Domain.Users.Enums;
using Nine.Identities.Domain.Users.Events;
using Nine.Identities.Domain.Users.ValueObjects;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.Entities;

public sealed class UserTests
{
    [Fact]
    public void CreateInstance_ShouldRaiseUserCreatedDomainEvent()
    {
        // Arrange
        var user = new UserBuilder().Build();

        // Act

        // Assert
        var userCreatedDomainEvent = (UserCreatedDomainEventV1)user.DomainEvents.Single();
        userCreatedDomainEvent.Name.Value.Should().Be(UserBuilder.DefaultNameValue);
        userCreatedDomainEvent.Email.Value.Should().Be(UserBuilder.DefaultEmailValue);
        userCreatedDomainEvent.PhoneNumber.Value.Should().Be(UserBuilder.DefaultPhoneValue);
        userCreatedDomainEvent.Username.Value.Should().Be(UserBuilder.DefaultUsernameValue);
        userCreatedDomainEvent.UserId.Should().NotBeNull();
        userCreatedDomainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetName_ShouldRaiseUserNameChangedDomainEvent()
    {
        // Arrange
        var user = new UserBuilder().WithoutCreationEvent().Build();
        var newName = Name.Create("Jane Doe");

        // Act
        user.SetName(newName);

        // Assert
        var userNameChangedDomainEvent = (UserNameChangedDomainEventV1)user.DomainEvents.Single();
        userNameChangedDomainEvent.UserId.Should().Be(user.UserId);
        userNameChangedDomainEvent.Name.Should().Be(newName);
        userNameChangedDomainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetEmail_ShouldRaiseUserEmailChangedDomainEvent()
    {
        // Arrange
        var user = new UserBuilder().WithoutCreationEvent().Build();
        var newEmail = Email.Create("jane@example.com");

        // Act
        user.SetEmail(newEmail);

        // Assert
        var userEmailChangedDomainEvent = (UserEmailChangedDomainEventV1)user.DomainEvents.Single();
        userEmailChangedDomainEvent.UserId.Should().Be(user.UserId);
        userEmailChangedDomainEvent.Email.Should().Be(newEmail);
        userEmailChangedDomainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetPhoneNumber_ShouldRaiseUserPhoneNumberChangedDomainEvent()
    {
        // Arrange
        var user = new UserBuilder().WithoutCreationEvent().Build();
        var newPhoneNumber = PhoneNumber.Create("+987654321");

        // Act
        user.SetPhoneNumber(newPhoneNumber);

        // Assert
        var userPhoneNumberChangedDomainEvent = (UserPhoneNumberChangedDomainEventV1)user.DomainEvents.Single();
        userPhoneNumberChangedDomainEvent.PhoneNumber.Should().Be(newPhoneNumber);
        userPhoneNumberChangedDomainEvent.UserId.Should().Be(user.UserId);
        userPhoneNumberChangedDomainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetUsername_ShouldRaiseUserUsernameChangedDomainEvent()
    {
        // Arrange
        var user = new UserBuilder().WithoutCreationEvent().Build();
        var newUsername = Username.Create("janedoe");

        // Act
        user.SetUsername(newUsername);

        // Assert
        var userUsernameChangedDomainEvent = (UserUsernameChangedDomainEventV1)user.DomainEvents.Single();
        userUsernameChangedDomainEvent.UserId.Should().Be(user.UserId);
        userUsernameChangedDomainEvent.Username.Should().Be(newUsername);
        userUsernameChangedDomainEvent.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ApplyingUserCreatedDomainEventV1_ShouldInitializeProperties()
    {
        // Arrange
        var userId = UserId.Create();
        var name = Name.Create("John Doe");
        var email = Email.Create("john@example.com");
        var phone = PhoneNumber.Create("+123456789");
        var username = Username.Create("johndoe");

        var userCreatedDomainEvent = new UserCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            UserId: userId,
            Name: name,
            Email: email,
            PhoneNumber: phone,
            Username: username,
            OccurredAt: DateTime.UtcNow
        );

        // Act
        var user = new User([userCreatedDomainEvent]);

        // Assert
        user.UserId.Should().Be(userId);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.PhoneNumber.Should().Be(phone);
        user.Username.Should().Be(username);
        user.State.Should().Be(UserStates.Active);
    }
}