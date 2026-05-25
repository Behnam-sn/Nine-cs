using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.Builders;

internal sealed class AccountTestBuilder
{
    public const string DefaultEmailAddressValue = "john@example.com";
    public const string DefaultPhoneNumberValue = "+123456789";
    public static readonly HashedSecret DefaultHashedPassword = HashedSecret.Create("hashedpassword");

    private EmailAddress _emailAddress;
    private PhoneNumber? _phoneNumber;
    private HashedSecret _hashedPassword;
    private bool _raiseCreationEvent = true;

    public AccountTestBuilder WithEmailAddress(string emailAddress)
    {
        _emailAddress = EmailAddress.Create(emailAddress);
        return this;
    }

    public AccountTestBuilder WithPhoneNumber(string phoneNumber)
    {
        _phoneNumber = PhoneNumber.Create(phoneNumber);
        return this;
    }

    public AccountTestBuilder WithPasswordCredential(HashedSecret hashedPassword)
    {
        _hashedPassword = hashedPassword;
        return this;
    }

    public AccountTestBuilder WithoutCreationEvent()
    {
        _raiseCreationEvent = false;
        return this;
    }

    public AccountTestBuilder WithPasswordRequiredParameters()
    {
        WithEmailAddress(DefaultEmailAddressValue);
        WithPasswordCredential(DefaultHashedPassword);
        return this;
    }

    public AccountTestBuilder WithPasswordOptionalParameters()
    {
        WithPhoneNumber(DefaultPhoneNumberValue);
        return this;
    }

    public Account BuildWithPassword()
    {
        var account = Account.CreateWithPassword(
            emailAddress: _emailAddress,
            phoneNumber: _phoneNumber,
            hashedPassword: _hashedPassword
        );

        if (!_raiseCreationEvent)
        {
            account.ClearDomainEvents();
        }

        return account;
    }
}
