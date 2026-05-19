using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.Entities;

internal sealed class AccountTestBuilder
{
    public const string DefaultEmailValue = "john@example.com";
    public const string DefaultPhoneValue = "+123456789";

    private Email _email;
    private PhoneNumber? _phone;

    private bool _raiseCreationEvent = true;

    public AccountTestBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }

    public AccountTestBuilder WithPhoneNumber(string phoneNumber)
    {
        _phone = PhoneNumber.Create(phoneNumber);
        return this;
    }

    public AccountTestBuilder WithoutCreationEvent()
    {
        _raiseCreationEvent = false;
        return this;
    }

    public AccountTestBuilder WithRequiredParameters()
    {
        WithEmail(DefaultEmailValue);
        return this;
    }

    public AccountTestBuilder WithOptionalParameters()
    {
        WithPhoneNumber(DefaultPhoneValue);
        return this;
    }

    public Account Build()
    {
        var account = Account.CreateInstance(_email, _phone);

        if (!_raiseCreationEvent)
        {
            account.ClearDomainEvents();
        }

        return account;
    }
}