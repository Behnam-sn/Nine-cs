using FluentAssertions;

using Nine.SharedKernel.Abstractions.ValueObjects;
using Nine.Identities.Domain.Users.Entities;
using Nine.Identities.Domain.Users.Enums;
using Nine.Identities.Domain.Users.Events;
using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.Entities;

public sealed class UserApplyDomainEventTests
{
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