using Nine.Identities.Domain.Accounts.Entities;
using Nine.Identities.Domain.Contracts.Accounts.ValueObjects;

namespace Nine.Identities.Domain.Tests.Accounts.Builders;

internal sealed class AccountTestBuilder
{
    public const string DefaultEmailAddressValue = "john@example.com";
    public const string DefaultPhoneNumberValue = "+123456789";
    public const string DefaultHashedPasswordValue = "hashedpassword";
    
    public static readonly EmailAddress DefaultEmailAddress = EmailAddress.Create(DefaultEmailAddressValue);
    public static readonly PhoneNumber DefaultPhoneNumber =  PhoneNumber.Create(DefaultPhoneNumberValue);
    public static readonly HashedSecret DefaultHashedPassword = HashedSecret.Create(DefaultHashedPasswordValue);

    private EmailAddress _emailAddress;
    private PhoneNumber? _phoneNumber;
    private HashedSecret _hashedPassword;
    private bool _raiseCreationEvent = true;

    public AccountTestBuilder WithEmailAddress(EmailAddress emailAddress)
    {
        _emailAddress = emailAddress;
        return this;
    }

    public AccountTestBuilder WithPhoneNumber(PhoneNumber phoneNumber)
    {
        _phoneNumber = phoneNumber;
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
        WithEmailAddress(DefaultEmailAddress);
        WithPasswordCredential(DefaultHashedPassword);
        return this;
    }

    public AccountTestBuilder WithPasswordOptionalParameters()
    {
        WithPhoneNumber(DefaultPhoneNumber);
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
