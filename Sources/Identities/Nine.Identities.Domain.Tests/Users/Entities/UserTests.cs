using FluentAssertions;

using Nine.Identities.Domain.Users.Events;

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
}