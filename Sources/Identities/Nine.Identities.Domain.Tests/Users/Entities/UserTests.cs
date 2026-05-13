using FluentAssertions;

using Nine.Identities.Domain.Users.Events;
using Nine.Identities.Domain.Users.ValueObjects;

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
        userCreatedDomainEvent!.Name.Value.Should().Be(UserBuilder.DefaultNameValue);
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
}