using FluentAssertions;

using Nine.Profiles.Domain.Contracts.Profiles.Events;
using Nine.Profiles.Domain.Contracts.Profiles.ValueObjects;
using Nine.Profiles.Domain.Profiles.Entities;
using Nine.Profiles.Domain.Tests.Profiles.Builders;
using Nine.SharedKernel.Abstractions.ValueObjects;

namespace Nine.Profiles.Domain.Tests.Profiles.Entities;

public sealed class ProfileTests
{
    #region Create

    [Fact]
    public void Create_WithRequiredParameters_ShouldRaiseAndApplyProfileCreatedDomainEventV1()
    {
        // Arrange
        var profile = new ProfileTestBuilder()
            .WithRequiredParameters()
            .Build();

        // Act

        // Assert
        var domainEvent = (ProfileCreatedDomainEventV1)profile.DomainEvents.Single();
        domainEvent.ProfileId.Should().NotBeNull();
        domainEvent.Name.Should().Be(ProfileTestBuilder.DefaultName);
        domainEvent.Handle.Should().Be(ProfileTestBuilder.DefaultHandle);
        domainEvent.Avatar.Should().BeNull();
        domainEvent.Bio.Should().Be(ProfileTestBuilder.DefaultBio);
        domainEvent.OwnerId.Should().Be(ProfileTestBuilder.DefaultOwnerId);
        domainEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithAllParameters_ShouldRaiseAndApplyProfileCreatedDomainEventV1()
    {
        // Arrange
        var profile = new ProfileTestBuilder()
            .WithRequiredParameters()
            .WithOptionalParameters()
            .Build();

        // Act

        // Assert
        var domainEvent = (ProfileCreatedDomainEventV1)profile.DomainEvents.Single();
        domainEvent.ProfileId.Should().NotBeNull();
        domainEvent.Name.Should().Be(ProfileTestBuilder.DefaultName);
        domainEvent.Handle.Should().Be(ProfileTestBuilder.DefaultHandle);
        domainEvent.Avatar.Should().Be(ProfileTestBuilder.DefaultAvatar);
        domainEvent.Bio.Should().Be(ProfileTestBuilder.DefaultBio);
        domainEvent.OwnerId.Should().Be(ProfileTestBuilder.DefaultOwnerId);
        domainEvent.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Rehydrate

    [Fact]
    public void Constructor_WithProfileCreatedDomainEventV1_ShouldApplyDomainEvent()
    {
        // Arrange
        var domainEvent = new ProfileCreatedDomainEventV1(
            Id: DomainEventId.Create(),
            ProfileId: ProfileId.Create(),
            Name: ProfileTestBuilder.DefaultName,
            Handle: ProfileTestBuilder.DefaultHandle,
            Avatar: null,
            Bio: ProfileTestBuilder.DefaultBio,
            OwnerId: ProfileTestBuilder.DefaultOwnerId,
            Timestamp: DateTime.UtcNow
        );

        // Act
        var profile = new Profile([domainEvent]);

        // Assert
        profile.ProfileId.Should().Be(domainEvent.ProfileId);
        profile.Name.Should().Be(domainEvent.Name);
        profile.Handle.Should().Be(domainEvent.Handle);
        profile.Avatar.Should().BeNull();
        profile.Bio.Should().Be(domainEvent.Bio);
        profile.OwnerId.Should().Be(domainEvent.OwnerId);
        profile.DomainEvents.Should().BeEmpty();
    }

    #endregion
}
