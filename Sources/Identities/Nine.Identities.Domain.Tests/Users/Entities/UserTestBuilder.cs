using Nine.Identities.Domain.Users.Entities;
using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.Entities;

internal sealed class UserTestBuilder
{
    public const string DefaultNameValue = "John Doe";
    public const string DefaultEmailValue = "john@example.com";
    public const string DefaultPhoneValue = "+123456789";

    private Name _name = Name.Create(DefaultNameValue);
    private Email _email = Email.Create(DefaultEmailValue);
    private PhoneNumber _phone = PhoneNumber.Create(DefaultPhoneValue);

    private bool _raiseCreationEvent = true;

    public UserTestBuilder WithName(string name)
    {
        _name = Name.Create(name);
        return this;
    }

    public UserTestBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }

    public UserTestBuilder WithPhoneNumber(string phone)
    {
        _phone = PhoneNumber.Create(phone);
        return this;
    }

    public UserTestBuilder WithoutCreationEvent()
    {
        _raiseCreationEvent = false;
        return this;
    }

    public User Build()
    {
        var user = User.CreateInstance(_name, _email, _phone);

        if (!_raiseCreationEvent)
        {
            user.ClearDomainEvents();
        }

        return user;
    }
}