using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.Entities;

internal sealed class AccountTestBuilder
{
    public const string DefaultEmailValue = "john@example.com";
    public const string DefaultPhoneValue = "+123456789";

    private Email _email = Email.Create(DefaultEmailValue);
    private PhoneNumber _phone = PhoneNumber.Create(DefaultPhoneValue);

    private bool _raiseCreationEvent = true;

    public AccountTestBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }

    public AccountTestBuilder WithPhoneNumber(string phone)
    {
        _phone = PhoneNumber.Create(phone);
        return this;
    }

    public AccountTestBuilder WithoutCreationEvent()
    {
        _raiseCreationEvent = false;
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