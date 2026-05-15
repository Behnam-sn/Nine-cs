using FluentAssertions;

using Nine.Identities.Domain.Users.Events;
using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.Entities;

public sealed class UserTests
{
    [Fact]
    public void CreateInstance_ShouldRaiseUserCreatedDomainEventV1AndUpdateState()
    {
        // Arrange
        var user = new UserBuilder().Build();

        // Act

        // Assert
        var userCreatedDomainEventV1 = (UserCreatedDomainEventV1)user.DomainEvents.Single();
        userCreatedDomainEventV1.Name.Value.Should().Be(UserBuilder.DefaultNameValue);
        userCreatedDomainEventV1.Email.Value.Should().Be(UserBuilder.DefaultEmailValue);
        userCreatedDomainEventV1.PhoneNumber.Value.Should().Be(UserBuilder.DefaultPhoneValue);
        userCreatedDomainEventV1.Username.Value.Should().Be(UserBuilder.DefaultUsernameValue);
        userCreatedDomainEventV1.UserId.Should().NotBeNull();
        userCreatedDomainEventV1.OccurredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        user.Name.Value.Should().Be(UserBuilder.DefaultNameValue);
        user.Email.Value.Should().Be(UserBuilder.DefaultEmailValue);
        user.PhoneNumber.Value.Should().Be(UserBuilder.DefaultPhoneValue);
        user.Username.Value.Should().Be(UserBuilder.DefaultUsernameValue);
        user.UserId.Should().NotBeNull();
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
}