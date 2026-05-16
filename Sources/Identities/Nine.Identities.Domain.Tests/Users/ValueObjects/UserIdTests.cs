using FluentAssertions;

using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.ValueObjects;

public sealed class UserIdTests
{
    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        var id1 = UserId.Create();
        var id2 = UserId.Create();

        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }
    
    [Fact]
    public void Parse_ShouldReturnCorrectId()
    {
        var guid = Guid.NewGuid();
        var userId = UserId.Parse(guid);

        userId.Value.Should().Be(guid);
        userId.ToString().Should().Be(guid.ToString());
    }
}