using Nine.Identities.Domain.Users.Entities;
using Nine.Identities.Domain.Users.ValueObjects;

namespace Nine.Identities.Domain.Tests.Users.Entities;

public sealed class UserBuilder
{
    public const string DefaultNameValue = "John Doe";
    public const string DefaultEmailValue = "john@example.com";
    public const string DefaultPhoneValue = "+123456789";
    public const string DefaultUsernameValue = "johndoe";

    private Name _name = Name.Create(DefaultNameValue);
    private Email _email = Email.Create(DefaultEmailValue);
    private PhoneNumber _phone = PhoneNumber.Create(DefaultPhoneValue);
    private Username _username = Username.Create(DefaultUsernameValue);

    private bool _raiseCreationEvent = true;

    public UserBuilder WithName(string name)
    {
        _name = Name.Create(name);
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }

    public UserBuilder WithPhoneNumber(string phone)
    {
        _phone = PhoneNumber.Create(phone);
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = Username.Create(username);
        return this;
    }

    public UserBuilder WithoutCreationEvent()
    {
        _raiseCreationEvent = false;
        return this;
    }

    public User Build()
    {
        var user = User.CreateInstance(_name, _email, _phone, _username);

        if (!_raiseCreationEvent)
        {
            user.ClearDomainEvents();
        }

        return user;
    }
}